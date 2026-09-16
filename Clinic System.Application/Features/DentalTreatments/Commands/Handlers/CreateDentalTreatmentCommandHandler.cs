namespace Clinic_System.Application.Features.DentalTreatments.Commands.Handlers
{
    public class CreateDentalTreatmentCommandHandler : AppRequestHandler<CreateDentalTreatmentCommand, DentalTreatmentDTO>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public CreateDentalTreatmentCommandHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<DentalTreatmentDTO>> Handle(CreateDentalTreatmentCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<DentalTreatmentDTO>("Only doctors or admins can register treatments.");

            var teeth = (request.ToothNumbers ?? [])
                .Where(FdiToothNumber.IsValid)
                .Distinct()
                .ToList();
            if (teeth.Count == 0 && request.ToothNumber is int single && FdiToothNumber.IsValid(single))
                teeth.Add(single);

            TreatmentProcedure? catalog = null;
            if (request.TreatmentProcedureId.HasValue)
            {
                catalog = await unitOfWork.TreatmentProceduresRepository.GetByIdAsync(
                    request.TreatmentProcedureId.Value, cancellationToken);
                if (catalog == null)
                    return NotFound<DentalTreatmentDTO>("No se encontró el procedimiento del catálogo.");
            }

            var target = catalog?.Target
                ?? (teeth.Count > 0 ? TreatmentProcedureTarget.PerTooth : TreatmentProcedureTarget.WholeMouth);

            DentalTreatment? last = null;
            try
            {
                if (target == TreatmentProcedureTarget.WholeMouth)
                {
                    last = await dentalTreatmentService.CreateAsync(
                        request.PatientId,
                        request.ProcedureName,
                        request.Cost,
                        request.AppointmentId,
                        null,
                        null,
                        request.TreatmentProcedureId,
                        request.ProcedureDetails,
                        request.MedicalNotes,
                        CurrentUserId,
                        cancellationToken);
                }
                else
                {
                    if (teeth.Count == 0)
                        return BadRequest<DentalTreatmentDTO>("Seleccione una o más piezas en el odontograma.");

                    foreach (var tooth in teeth)
                    {
                        last = await dentalTreatmentService.CreateAsync(
                            request.PatientId,
                            request.ProcedureName,
                            request.Cost,
                            request.AppointmentId,
                            tooth,
                            request.ToothSurface,
                            request.TreatmentProcedureId,
                            request.ProcedureDetails,
                            request.MedicalNotes,
                            CurrentUserId,
                            cancellationToken);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<DentalTreatmentDTO>(ex.Message);
            }

            await unitOfWork.SaveAsync(cancellationToken);
            var message = target == TreatmentProcedureTarget.WholeMouth || teeth.Count <= 1
                ? "Tratamiento registrado."
                : $"Tratamiento registrado en {teeth.Count} piezas.";
            return Success(mapper.Map<DentalTreatmentDTO>(last!), message);
        }
    }
}
