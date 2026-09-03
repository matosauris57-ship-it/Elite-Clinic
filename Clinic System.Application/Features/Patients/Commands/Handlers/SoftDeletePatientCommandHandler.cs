namespace Clinic_System.Application.Features.Patients.Commands.Handlers
{
    public class SoftDeletePatientCommandHandler : ResponseHandler, IRequestHandler<SoftDeletePatientCommand, Response<Patient>>
    {
        private readonly IPatientService patientService;
        private readonly IIdentityService identityService;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<SoftDeletePatientCommandHandler> logger;

        public SoftDeletePatientCommandHandler(
            ICurrentUserService @object,
            IPatientService patientService,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            ILogger<SoftDeletePatientCommandHandler> logger)
        {
            this.patientService = patientService;
            this.identityService = identityService;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        public async Task<Response<Patient>> Handle(SoftDeletePatientCommand request, CancellationToken cancellationToken)
        {
            var patient = await patientService.GetPatientByIdIncludingDeletedAsync(request.Id, cancellationToken);

            if (patient == null)
                return NotFound<Patient>($"No se encontró el paciente con Id {request.Id}");

            if (patient.IsDeleted)
                return BadRequest<Patient>("El paciente ya está deshabilitado");

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            try
            {
                await patientService.SoftDeletePatient(patient, cancellationToken);

                if (await unitOfWork.SaveAsync(cancellationToken) == 0)
                    return BadRequest<Patient>("No se pudo deshabilitar el paciente");

                if (!string.IsNullOrEmpty(patient.ApplicationUserId) &&
                    !await identityService.SoftDeleteUserAsync(patient.ApplicationUserId, cancellationToken))
                    return BadRequest<Patient>("No se pudo deshabilitar la cuenta de acceso del paciente");

                transaction.Complete();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deshabilitando paciente {PatientId}", request.Id);
                return BadRequest<Patient>($"No se pudo deshabilitar el paciente: {ex.Message}");
            }

            return Deleted<Patient>("Paciente deshabilitado correctamente");
        }
    }
}
