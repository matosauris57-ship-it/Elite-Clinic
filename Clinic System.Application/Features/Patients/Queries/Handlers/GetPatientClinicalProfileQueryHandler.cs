namespace Clinic_System.Application.Features.Patients.Queries.Handlers
{
    public class GetPatientClinicalProfileQueryHandler : ResponseHandler,
        IRequestHandler<GetPatientClinicalProfileQuery, Response<PatientClinicalProfileDTO>>
    {
        private readonly IPatientService patientService;
        private readonly IDentalHistoryService dentalHistoryService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public GetPatientClinicalProfileQueryHandler(
            IPatientService patientService,
            IDentalHistoryService dentalHistoryService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            this.patientService = patientService;
            this.dentalHistoryService = dentalHistoryService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Response<PatientClinicalProfileDTO>> Handle(GetPatientClinicalProfileQuery request, CancellationToken cancellationToken)
        {
            var patient = await patientService.GetPatientByIdIncludingDeletedAsync(request.PatientId, cancellationToken);
            if (patient == null)
                return NotFound<PatientClinicalProfileDTO>($"No se encontró el paciente con Id {request.PatientId}");

            var profile = new PatientClinicalProfileDTO
            {
                Id = patient.Id,
                FullName = patient.FullName,
                Gender = patient.Gender.ToString(),
                DateOfBirth = patient.DateOfBirth.ToString("dd/MM/yyyy"),
                DateOfBirthIso = patient.DateOfBirth.ToString("yyyy-MM-dd"),
                Address = patient.Address,
                Phone = patient.Phone,
                NationalId = patient.NationalId,
                MobilePhone = patient.MobilePhone,
                Email = patient.Email,
                OptOutEmailCampaigns = patient.OptOutEmailCampaigns,
                IsActive = !patient.IsDeleted
            };

            var history = await dentalHistoryService.GetByPatientIdAsync(request.PatientId, cancellationToken);
            if (history != null)
            {
                profile.DentalHistory = mapper.Map<DentalHistoryDTO>(history);
                var conditions = await unitOfWork.PatientMedicalConditionsRepository.GetByPatientIdAsync(request.PatientId, cancellationToken);
                profile.DentalHistory.SelectedConditionIds = conditions.Select(c => c.MedicalConditionId).ToList();
            }
            else
            {
                profile.DentalHistory = new DentalHistoryDTO { PatientId = request.PatientId };
            }

            return Success(profile);
        }
    }
}
