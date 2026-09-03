namespace Clinic_System.Application.Features.DentalTreatments.Queries.Handlers
{
    public class GetDentalTreatmentByIdQueryHandler : AppRequestHandler<GetDentalTreatmentByIdQuery, DentalTreatmentListItemDTO>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IMapper mapper;

        public GetDentalTreatmentByIdQueryHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IMapper mapper) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.mapper = mapper;
        }

        public override async Task<Response<DentalTreatmentListItemDTO>> Handle(GetDentalTreatmentByIdQuery request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<DentalTreatmentListItemDTO>("Access denied.");

            var treatment = await dentalTreatmentService.GetByIdAsync(request.Id, cancellationToken);
            return Success(mapper.Map<DentalTreatmentListItemDTO>(treatment));
        }
    }
}
