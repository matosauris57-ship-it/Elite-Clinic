namespace Clinic_System.Application.Features.Patients.Commands.Handlers
{
    public class CreatePatientCommandHandler : ResponseHandler, IRequestHandler<CreatePatientCommand, Response<CreatePatientDTO>>
    {
        private readonly IPatientService patientService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<CreatePatientCommandHandler> logger;

        public CreatePatientCommandHandler(
            IPatientService patientService,
            IMapper mapper,
            IUnitOfWork unitOfWork,
            ILogger<CreatePatientCommandHandler> logger)
        {
            this.patientService = patientService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Response<CreatePatientDTO>> Handle(CreatePatientCommand request, CancellationToken cancellationToken)
        {
            Patient? patient = null;

            logger.LogInformation("Starting the process to add a new patient with name: {PatientName}", request.FullName);
            try
            {
                patient = mapper.Map<Patient>(request);
                patient.ApplicationUserId = null;
                patient.Email = ContactEmail.NormalizeOrNull(request.Email);
                patient.NationalId = string.IsNullOrWhiteSpace(request.NationalId) ? null : request.NationalId.Trim();

                await patientService.CreatePatientAsync(patient, cancellationToken);
                var result = await unitOfWork.SaveAsync();
                if (result == 0)
                {
                    logger.LogWarning("Failed to save the patient {PatientName} to the database", request.FullName);
                    return BadRequest<CreatePatientDTO>("Failed to create patient");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while adding patient: {PatientName}", request.FullName);
                return BadRequest<CreatePatientDTO>($"Patient creation failed: {ex.Message}");
            }

            var dto = mapper.Map<CreatePatientDTO>(patient);
            var locationUri = $"/api/patients/id/{patient.Id}";
            logger.LogInformation("Patient {PatientName} added successfully with ID: {PatientId}", request.FullName, patient.Id);
            return Created(dto, locationUri);
        }
    }
}
