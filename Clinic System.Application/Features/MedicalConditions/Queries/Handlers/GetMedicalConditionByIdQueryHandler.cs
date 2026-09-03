namespace Clinic_System.Application.Features.MedicalConditions.Queries.Handlers
{
    public class GetMedicalConditionByIdQueryHandler : ResponseHandler,
        IRequestHandler<GetMedicalConditionByIdQuery, Response<MedicalConditionDTO>>
    {
        private readonly IMedicalConditionService medicalConditionService;
        private readonly IMapper mapper;

        public GetMedicalConditionByIdQueryHandler(IMedicalConditionService medicalConditionService, IMapper mapper)
        {
            this.medicalConditionService = medicalConditionService;
            this.mapper = mapper;
        }

        public async Task<Response<MedicalConditionDTO>> Handle(GetMedicalConditionByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var item = await medicalConditionService.GetByIdAsync(request.Id, cancellationToken);
                return Success(mapper.Map<MedicalConditionDTO>(item));
            }
            catch (NotFoundException)
            {
                return NotFound<MedicalConditionDTO>($"No se encontró la enfermedad con Id {request.Id}");
            }
        }
    }
}
