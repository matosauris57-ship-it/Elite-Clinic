namespace Clinic_System.Application.Features.DentalHistory.Queries.Handlers
{
    public class GetDentalHistoryByPatientQueryHandler : AppRequestHandler<GetDentalHistoryByPatientQuery, DentalHistoryDTO>
    {
        private readonly IDentalHistoryService dentalHistoryService;
        private readonly IMapper mapper;

        public GetDentalHistoryByPatientQueryHandler(
            ICurrentUserService currentUserService,
            IDentalHistoryService dentalHistoryService,
            IMapper mapper) : base(currentUserService)
        {
            this.dentalHistoryService = dentalHistoryService;
            this.mapper = mapper;
        }

        public override async Task<Response<DentalHistoryDTO>> Handle(GetDentalHistoryByPatientQuery request, CancellationToken cancellationToken)
        {
            int patientId;
            if (CurrentDoctorId.HasValue)
            {
                if (request.PatientId <= 0)
                    return BadRequest<DentalHistoryDTO>("PatientId is required for doctors.");
                patientId = request.PatientId;
            }
            else
            {
                var (authorizedPatientId, error) = await GetAuthorizedPatientId(request.PatientId);
                if (error != null) return error;
                patientId = authorizedPatientId;
            }

            var history = await dentalHistoryService.GetByPatientIdAsync(patientId, cancellationToken);
            if (history == null)
                return NotFound<DentalHistoryDTO>("Dental history not found for this patient.");

            return Success(mapper.Map<DentalHistoryDTO>(history));
        }
    }
}
