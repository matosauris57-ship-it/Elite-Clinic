namespace Clinic_System.Application.Service.Implemention
{
    public class TreatmentProcedureService : ITreatmentProcedureService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public TreatmentProcedureService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public Task<IEnumerable<TreatmentProcedure>> GetAllAsync(bool activeOnly, CancellationToken cancellationToken = default)
            => unitOfWork.TreatmentProceduresRepository.GetAllAsync(activeOnly, cancellationToken);

        public async Task<TreatmentProcedure> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            var procedure = await unitOfWork.TreatmentProceduresRepository.GetByIdAsync(id, cancellationToken);
            if (procedure == null)
                throw new NotFoundException($"Treatment procedure with ID {id} not found.");
            return procedure;
        }

        public async Task<TreatmentProcedure> CreateAsync(
            string code,
            string category,
            string name,
            decimal price,
            TreatmentPricingMode pricingMode,
            int durationMinutes,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var normalizedCode = code.Trim().ToLowerInvariant();
            var existing = await unitOfWork.TreatmentProceduresRepository.GetByCodeAsync(normalizedCode, cancellationToken);
            if (existing != null)
                throw new InvalidOperationException($"A procedure with code '{normalizedCode}' already exists.");

            var normalizedPrice = pricingMode == TreatmentPricingMode.Fixed
                ? Money.Normalize(price)
                : 0m;

            var procedure = new TreatmentProcedure
            {
                Code = normalizedCode,
                Category = category.Trim().ToUpperInvariant(),
                Name = name.Trim(),
                Price = normalizedPrice,
                PricingMode = pricingMode,
                DurationMinutes = durationMinutes,
                IsActive = isActive
            };

            await unitOfWork.TreatmentProceduresRepository.AddAsync(procedure, cancellationToken);
            return procedure;
        }

        public async Task<TreatmentProcedure> UpdateAsync(
            int id,
            string code,
            string category,
            string name,
            decimal price,
            TreatmentPricingMode pricingMode,
            int durationMinutes,
            bool isActive,
            CancellationToken cancellationToken = default)
        {
            var procedure = await GetByIdAsync(id, cancellationToken);
            var normalizedCode = code.Trim().ToLowerInvariant();
            var existing = await unitOfWork.TreatmentProceduresRepository.GetByCodeAsync(normalizedCode, cancellationToken);
            if (existing != null && existing.Id != id)
                throw new InvalidOperationException($"A procedure with code '{normalizedCode}' already exists.");

            procedure.Code = normalizedCode;
            procedure.Category = category.Trim().ToUpperInvariant();
            procedure.Name = name.Trim();
            procedure.PricingMode = pricingMode;
            procedure.Price = pricingMode == TreatmentPricingMode.Fixed
                ? Money.Normalize(price)
                : 0m;
            procedure.DurationMinutes = durationMinutes;
            procedure.IsActive = isActive;

            unitOfWork.TreatmentProceduresRepository.Update(procedure, cancellationToken);
            return procedure;
        }

        public async Task SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var procedure = await GetByIdAsync(id, cancellationToken);
            procedure.IsDeleted = true;
            procedure.DeletedAt = DateTime.Now;
            unitOfWork.TreatmentProceduresRepository.Update(procedure, cancellationToken);
        }

        public async Task ReplaceDoctorPricesAsync(
            int procedureId,
            IEnumerable<DoctorProcedurePriceInput> prices,
            CancellationToken cancellationToken = default)
        {
            await GetByIdAsync(procedureId, cancellationToken);

            var incoming = (prices ?? [])
                .GroupBy(p => p.DoctorId)
                .Select(g => g.Last())
                .ToList();

            if (incoming.Count > 0)
            {
                var doctorIds = incoming.Select(p => p.DoctorId).ToList();
                var doctors = await unitOfWork.DoctorsRepository.FindAsync(d => doctorIds.Contains(d.Id), cancellationToken);
                var foundIds = doctors.Select(d => d.Id).ToHashSet();
                var missing = doctorIds.Where(id => !foundIds.Contains(id)).ToList();
                if (missing.Count > 0)
                    throw new NotFoundException($"Doctor(s) not found: {string.Join(", ", missing)}.");
            }

            var existing = await unitOfWork.DoctorProcedurePricesRepository.GetByProcedureIdAsync(procedureId, cancellationToken);
            if (existing.Count > 0)
                unitOfWork.DoctorProcedurePricesRepository.RemoveRange(existing);

            foreach (var price in incoming)
            {
                await unitOfWork.DoctorProcedurePricesRepository.AddAsync(new DoctorProcedurePrice
                {
                    DoctorId = price.DoctorId,
                    TreatmentProcedureId = procedureId,
                    Price = Money.Normalize(price.Price)
                }, cancellationToken);
            }
        }

        public async Task<decimal> ResolvePriceAsync(int procedureId, int? doctorId, CancellationToken cancellationToken = default)
        {
            var procedure = await GetByIdAsync(procedureId, cancellationToken);
            if (doctorId.HasValue)
            {
                var doctorPrice = await unitOfWork.DoctorProcedurePricesRepository.GetAsync(
                    doctorId.Value, procedureId, cancellationToken);
                if (doctorPrice != null)
                    return doctorPrice.Price;
            }

            return procedure.Price;
        }

        public async Task<TreatmentProcedureDTO> ToDtoAsync(TreatmentProcedure procedure, int? doctorId, CancellationToken cancellationToken = default)
        {
            var dtos = await ToDtosAsync([procedure], doctorId, cancellationToken);
            return dtos[0];
        }

        public async Task<List<TreatmentProcedureDTO>> ToDtosAsync(
            IEnumerable<TreatmentProcedure> procedures,
            int? doctorId,
            CancellationToken cancellationToken = default)
        {
            var list = procedures.ToList();
            var dtos = mapper.Map<List<TreatmentProcedureDTO>>(list);
            if (dtos.Count == 0)
                return dtos;

            var ids = dtos.Select(d => d.Id).ToList();
            var prices = await unitOfWork.DoctorProcedurePricesRepository.GetByProcedureIdsAsync(ids, cancellationToken);
            var doctors = (await unitOfWork.DoctorsRepository.GetAllAsync(cancellationToken))
                .ToDictionary(d => d.Id, d => d.FullName);

            var pricesByProcedure = prices
                .Where(p => doctors.ContainsKey(p.DoctorId))
                .GroupBy(p => p.TreatmentProcedureId)
                .ToDictionary(g => g.Key, g => g.OrderBy(p => doctors[p.DoctorId]).ToList());

            foreach (var dto in dtos)
            {
                var source = list.First(p => p.Id == dto.Id);
                dto.PricingMode = source.PricingMode.ToString();
                dto.PricingModeDisplay = FormatPricingMode(source.PricingMode);

                var procedurePrices = pricesByProcedure.GetValueOrDefault(dto.Id) ?? [];
                dto.DoctorPrices = procedurePrices.Select(p => new DoctorProcedurePriceDTO
                {
                    DoctorId = p.DoctorId,
                    DoctorName = doctors[p.DoctorId],
                    Price = p.Price,
                    PriceDisplay = Money.Format(p.Price),
                    PriceRaw = Money.ToInput(p.Price)
                }).ToList();

                decimal catalogPrice = source.Price;
                decimal resolved;
                if (source.PricingMode == TreatmentPricingMode.Fixed)
                {
                    if (doctorId.HasValue)
                    {
                        var match = procedurePrices.FirstOrDefault(p => p.DoctorId == doctorId.Value);
                        resolved = match?.Price ?? catalogPrice;
                    }
                    else if (procedurePrices.Count > 0)
                    {
                        resolved = procedurePrices.Min(p => p.Price);
                    }
                    else
                    {
                        resolved = catalogPrice;
                    }

                    dto.Price = resolved;
                    dto.PriceRaw = Money.ToInput(resolved);
                    dto.PriceRangeDisplay = procedurePrices.Count > 0
                        ? Money.FormatRange(procedurePrices.Min(p => p.Price), procedurePrices.Max(p => p.Price))
                        : Money.Format(resolved);
                    dto.PriceDisplay = dto.PriceRangeDisplay;
                }
                else
                {
                    dto.Price = 0;
                    dto.PriceRaw = string.Empty;
                    dto.PriceRangeDisplay = dto.PricingModeDisplay;
                    dto.PriceDisplay = dto.PricingModeDisplay;
                }
            }

            return dtos;
        }

        private static string FormatPricingMode(TreatmentPricingMode mode) => mode switch
        {
            TreatmentPricingMode.Fixed => "Precio fijo",
            TreatmentPricingMode.AtBooking => "Al agendar",
            TreatmentPricingMode.AtBilling => "Al facturar",
            _ => mode.ToString()
        };
    }
}
