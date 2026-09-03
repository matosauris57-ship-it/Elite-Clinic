namespace Clinic_System.Application.Features.DentalTreatments.Commands.Handlers
{
    public class UpdateDentalTreatmentCommandHandler : AppRequestHandler<UpdateDentalTreatmentCommand, DentalTreatmentDTO>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IMapper mapper;
        private readonly IUnitOfWork unitOfWork;

        public UpdateDentalTreatmentCommandHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IMapper mapper,
            IUnitOfWork unitOfWork) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.mapper = mapper;
            this.unitOfWork = unitOfWork;
        }

        public override async Task<Response<DentalTreatmentDTO>> Handle(UpdateDentalTreatmentCommand request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin") && !roles.Contains("Doctor"))
                return Unauthorized<DentalTreatmentDTO>("Only doctors or admins can update treatments.");

            var treatment = await dentalTreatmentService.UpdateAsync(
                request.Id,
                request.ProcedureName,
                request.Cost,
                request.ToothNumber,
                request.ToothSurface,
                request.TreatmentProcedureId,
                request.ProcedureDetails,
                cancellationToken);

            await unitOfWork.SaveAsync(cancellationToken);
            return Success(mapper.Map<DentalTreatmentDTO>(treatment), "Dental treatment updated.");
        }
    }
}
