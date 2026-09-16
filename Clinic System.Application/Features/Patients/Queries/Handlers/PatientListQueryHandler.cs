namespace Clinic_System.Application.Features.Patients.Queries.Handlers
{
    public class PatientListQueryHandler : ResponseHandler, IRequestHandler<GetPatientListQuery, Response<List<GetPatientListDTO>>>
    {
        private readonly IPatientService patientService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public PatientListQueryHandler(
            IPatientService patientService,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            this.patientService = patientService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Response<List<GetPatientListDTO>>> Handle(GetPatientListQuery request, CancellationToken cancellationToken)
        {
            var patients = await patientService.GetPatientsListForAdminAsync(
                request.IncludeInactive,
                cancellationToken);
            var mapped = mapper.Map<List<GetPatientListDTO>>(patients.Where(p => p != null));
            var balances = await unitOfWork.PaymentsRepository.GetOutstandingBalancesByPatientAsync(cancellationToken);
            foreach (var patient in mapped)
            {
                if (balances.TryGetValue(patient.Id, out var amount))
                    patient.OutstandingBalance = amount;
            }

            return Success(mapped);
        }
    }
}
