namespace Clinic_System.Application.Features.Patients.Queries.Handlers
{
    public class PatientListPagingQueryHandler : ResponseHandler, IRequestHandler<GetPatientListPagingQuery, Response<PagedResult<GetPatientListDTO>>>
    {
        private readonly IPatientService patientService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<PatientListPagingQueryHandler> logger;

        public PatientListPagingQueryHandler(
            IPatientService patientService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<PatientListPagingQueryHandler> logger)
        {
            this.patientService = patientService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Response<PagedResult<GetPatientListDTO>>> Handle(GetPatientListPagingQuery request, CancellationToken cancellationToken)
        {
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 20 : Math.Min(request.PageSize, 100);
            var status = string.IsNullOrWhiteSpace(request.Status) ? "all" : request.Status.Trim();
            var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

            logger.LogInformation(
                "Handling GetPatientListPagingQuery: PageNumber={PageNumber}, PageSize={PageSize}, Status={Status}, Search={Search}",
                pageNumber, pageSize, status, search);

            var patients = await patientService.GetPatientsListPagingAsync(
                pageNumber,
                pageSize,
                search,
                status,
                attendedByDoctorId: null,
                cancellationToken);

            var patientsMapper = mapper.Map<List<GetPatientListDTO>>(patients.Items);
            if (patientsMapper.Count > 0)
            {
                var balances = await unitOfWork.PaymentsRepository.GetOutstandingBalancesByPatientAsync(
                    patientsMapper.Select(p => p.Id),
                    cancellationToken);
                foreach (var patient in patientsMapper)
                {
                    if (balances.TryGetValue(patient.Id, out var amount))
                        patient.OutstandingBalance = amount;
                }
            }

            var pagedResult = new PagedResult<GetPatientListDTO>(
                patientsMapper,
                patients.TotalCount,
                patients.CurrentPage,
                patients.PageSize);

            logger.LogInformation(
                "Successfully retrieved {Count}/{Total} patients for PageNumber={PageNumber}, PageSize={PageSize}",
                patientsMapper.Count, patients.TotalCount, pageNumber, pageSize);

            return Success(pagedResult);
        }
    }
}
