namespace Clinic_System.Application.Features.MedicalConditions.Commands.Handlers
{
    public class SoftDeleteMedicalConditionCommandHandler : ResponseHandler,
        IRequestHandler<SoftDeleteMedicalConditionCommand, Response<string>>
    {
        private readonly IMedicalConditionService medicalConditionService;
        private readonly IUnitOfWork unitOfWork;

        public SoftDeleteMedicalConditionCommandHandler(IMedicalConditionService medicalConditionService, IUnitOfWork unitOfWork)
        {
            this.medicalConditionService = medicalConditionService;
            this.unitOfWork = unitOfWork;
        }

        public async Task<Response<string>> Handle(SoftDeleteMedicalConditionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await medicalConditionService.SoftDeleteAsync(request.Id, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
                return Deleted<string>("Enfermedad eliminada correctamente");
            }
            catch (NotFoundException)
            {
                return NotFound<string>($"No se encontró la enfermedad con Id {request.Id}");
            }
        }
    }
}
