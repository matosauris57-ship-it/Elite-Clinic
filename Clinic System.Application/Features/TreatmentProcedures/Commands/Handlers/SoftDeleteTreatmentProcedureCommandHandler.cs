namespace Clinic_System.Application.Features.TreatmentProcedures.Commands.Handlers
{
    public class SoftDeleteTreatmentProcedureCommandHandler : AppRequestHandler<SoftDeleteTreatmentProcedureCommand, string>
    {
        private readonly ITreatmentProcedureService treatmentProcedureService;
        private readonly IUnitOfWork unitOfWork;

        public SoftDeleteTreatmentProcedureCommandHandler(
            ICurrentUserService currentUserService,
            ITreatmentProcedureService treatmentProcedureService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.treatmentProcedureService = treatmentProcedureService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<string>> Handle(SoftDeleteTreatmentProcedureCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin"))
                return Unauthorized<string>("Only admins can manage the treatment catalog.");

            await treatmentProcedureService.SoftDeleteAsync(request.Id, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            return Deleted<string>("Treatment procedure deleted.");
        }
    }
}
