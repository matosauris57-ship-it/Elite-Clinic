namespace Clinic_System.Application.Features.MedicalConditions.Queries.Handlers
{
    public class GetMedicalConditionListQueryHandler : ResponseHandler,
        IRequestHandler<GetMedicalConditionListQuery, Response<List<MedicalConditionDTO>>>
    {
        private readonly IMedicalConditionService medicalConditionService;
        private readonly IMapper mapper;

        public GetMedicalConditionListQueryHandler(IMedicalConditionService medicalConditionService, IMapper mapper)
        {
            this.medicalConditionService = medicalConditionService;
            this.mapper = mapper;
        }

        public async Task<Response<List<MedicalConditionDTO>>> Handle(GetMedicalConditionListQuery request, CancellationToken cancellationToken)
        {
            var items = await medicalConditionService.GetAllAsync(request.ActiveOnly, cancellationToken);
            return Success(mapper.Map<List<MedicalConditionDTO>>(items));
        }
    }
}
