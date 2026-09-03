namespace Clinic_System.Application.Features.ToothRecords.Queries.Handlers
{
    public class GetToothRecordsByPatientQueryHandler : AppRequestHandler<GetToothRecordsByPatientQuery, List<ToothRecordDTO>>
    {
        private readonly IToothRecordService toothRecordService;
        private readonly IMapper mapper;

        public GetToothRecordsByPatientQueryHandler(
            ICurrentUserService currentUserService,
            IToothRecordService toothRecordService,
            IMapper mapper) : base(currentUserService)
        {
            this.toothRecordService = toothRecordService;
            this.mapper = mapper;
        }

        public override async Task<Response<List<ToothRecordDTO>>> Handle(GetToothRecordsByPatientQuery request, CancellationToken cancellationToken)
        {
            int patientId;
            if (CurrentDoctorId.HasValue)
            {
                if (request.PatientId <= 0)
                    return BadRequest<List<ToothRecordDTO>>("PatientId is required for doctors.");
                patientId = request.PatientId;
            }
            else
            {
                var (authorizedPatientId, error) = await GetAuthorizedPatientId(request.PatientId);
                if (error != null) return error;
                patientId = authorizedPatientId;
            }

            var records = await toothRecordService.GetByPatientIdAsync(patientId, cancellationToken);
            return Success(mapper.Map<List<ToothRecordDTO>>(records.ToList()));
        }
    }
}
