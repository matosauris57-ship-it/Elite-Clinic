namespace Clinic_System.Application.Features.MedicalConditions.Commands.Handlers
{
    public class UpdateMedicalConditionCommandHandler : ResponseHandler,
        IRequestHandler<UpdateMedicalConditionCommand, Response<MedicalConditionDTO>>
    {
        private readonly IMedicalConditionService medicalConditionService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public UpdateMedicalConditionCommandHandler(
            IMedicalConditionService medicalConditionService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            this.medicalConditionService = medicalConditionService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Response<MedicalConditionDTO>> Handle(UpdateMedicalConditionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var condition = await medicalConditionService.UpdateAsync(
                    request.Id, request.Name, request.Category, request.IsActive, request.SortOrder, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
                return Success(mapper.Map<MedicalConditionDTO>(condition), "Enfermedad actualizada correctamente");
            }
            catch (NotFoundException)
            {
                return NotFound<MedicalConditionDTO>($"No se encontró la enfermedad con Id {request.Id}");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<MedicalConditionDTO>(ex.Message);
            }
        }
    }
}
