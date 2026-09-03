namespace Clinic_System.Application.Features.DentalTreatments.Commands.Handlers
{
    public class CreateDentalTreatmentCommandHandler : AppRequestHandler<CreateDentalTreatmentCommand, DentalTreatmentDTO>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public CreateDentalTreatmentCommandHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<DentalTreatmentDTO>> Handle(CreateDentalTreatmentCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<DentalTreatmentDTO>("Only doctors or admins can register treatments.");

            var treatment = await dentalTreatmentService.CreateAsync(
                request.PatientId,
                request.ProcedureName,
                request.Cost,
                request.AppointmentId,
                request.ToothNumber,
                request.ToothSurface,
                request.TreatmentProcedureId,
                request.ProcedureDetails,
                CurrentUserId,
                cancellationToken);

            await unitOfWork.SaveAsync(cancellationToken);
            return Success(mapper.Map<DentalTreatmentDTO>(treatment), "Dental treatment created.");
        }
    }
}
