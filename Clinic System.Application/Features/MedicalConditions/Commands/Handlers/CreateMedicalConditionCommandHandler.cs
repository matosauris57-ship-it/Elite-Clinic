namespace Clinic_System.Application.Features.MedicalConditions.Commands.Handlers
{
    public class CreateMedicalConditionCommandHandler : ResponseHandler,
        IRequestHandler<CreateMedicalConditionCommand, Response<MedicalConditionDTO>>
    {
        private readonly IMedicalConditionService medicalConditionService;
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public CreateMedicalConditionCommandHandler(
            IMedicalConditionService medicalConditionService,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            this.medicalConditionService = medicalConditionService;
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<Response<MedicalConditionDTO>> Handle(CreateMedicalConditionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var condition = await medicalConditionService.CreateAsync(
                    request.Name, request.Category, request.IsActive, request.SortOrder, cancellationToken);
                await unitOfWork.SaveAsync(cancellationToken);
                return Success(mapper.Map<MedicalConditionDTO>(condition), "Enfermedad creada correctamente");
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest<MedicalConditionDTO>(ex.Message);
            }
        }
    }
}
