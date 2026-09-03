namespace Clinic_System.Application.Features.DentalTreatments.Queries.Handlers
{
    public class GetDentalTreatmentsByAppointmentQueryHandler : AppRequestHandler<GetDentalTreatmentsByAppointmentQuery, List<DentalTreatmentDTO>>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IMapper mapper;

        public GetDentalTreatmentsByAppointmentQueryHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IMapper mapper) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.mapper = mapper;
        }

        public override async Task<Response<List<DentalTreatmentDTO>>> Handle(GetDentalTreatmentsByAppointmentQuery request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<List<DentalTreatmentDTO>>("Access denied.");

            var treatments = await dentalTreatmentService.GetByAppointmentIdAsync(request.AppointmentId, cancellationToken);
            return Success(mapper.Map<List<DentalTreatmentDTO>>(treatments.ToList()));
        }
    }
}
