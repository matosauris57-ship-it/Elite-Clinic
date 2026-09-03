namespace Clinic_System.Application.Features.TreatmentProcedures.Commands.Handlers
{
    public class CreateTreatmentProcedureCommandHandler : AppRequestHandler<CreateTreatmentProcedureCommand, TreatmentProcedureDTO>
    {
        private readonly ITreatmentProcedureService treatmentProcedureService;
        private readonly IUnitOfWork unitOfWork;

        public CreateTreatmentProcedureCommandHandler(
            ICurrentUserService currentUserService,
            ITreatmentProcedureService treatmentProcedureService,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.treatmentProcedureService = treatmentProcedureService;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<TreatmentProcedureDTO>> Handle(CreateTreatmentProcedureCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin"))
                return Unauthorized<TreatmentProcedureDTO>("Only admins can manage the treatment catalog.");

            var procedure = await treatmentProcedureService.CreateAsync(
                request.Code,
                request.Category,
                request.Name,
                request.Price,
                request.DurationMinutes,
                request.IsActive,
                cancellationToken);

            await unitOfWork.SaveAsync(cancellationToken);
            await treatmentProcedureService.ReplaceDoctorPricesAsync(procedure.Id, request.DoctorPrices, cancellationToken);
            await unitOfWork.SaveAsync(cancellationToken);
            return Success(await treatmentProcedureService.ToDtoAsync(procedure, null, cancellationToken), "Treatment procedure created.");
        }
    }
}
