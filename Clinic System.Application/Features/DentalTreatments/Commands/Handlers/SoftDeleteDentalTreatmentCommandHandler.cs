namespace Clinic_System.Application.Features.DentalTreatments.Commands.Handlers
{
    public class SoftDeleteDentalTreatmentCommandHandler : AppRequestHandler<SoftDeleteDentalTreatmentCommand, string>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IUnitOfWork unitOfWork;

        public SoftDeleteDentalTreatmentCommandHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<string>> Handle(SoftDeleteDentalTreatmentCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin"))
                return Unauthorized<string>("Only admins can delete treatments.");

            await dentalTreatmentService.SoftDeleteAsync(request.TreatmentId, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            return Deleted<string>("Dental treatment deleted.");
        }
    }
}
