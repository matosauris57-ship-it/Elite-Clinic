namespace Clinic_System.Application.Features.TreatmentProcedures.Queries.Handlers
{
    public class GetTreatmentProcedureByIdQueryHandler : AppRequestHandler<GetTreatmentProcedureByIdQuery, TreatmentProcedureDTO>
    {
        private readonly ITreatmentProcedureService treatmentProcedureService;

        public GetTreatmentProcedureByIdQueryHandler(
            ICurrentUserService currentUserService,
            ITreatmentProcedureService treatmentProcedureService) : base(currentUserService)
        {
            this.treatmentProcedureService = treatmentProcedureService;
        }

        public override async Task<Response<TreatmentProcedureDTO>> Handle(GetTreatmentProcedureByIdQuery request, CancellationToken cancellationToken)
        {
            var roles = await _currentUserService.GetCurrentUserRolesAsync();
            if (!roles.Contains("Admin"))
                return Unauthorized<TreatmentProcedureDTO>("Access denied.");

            var procedure = await treatmentProcedureService.GetByIdAsync(request.Id, cancellationToken);
            return Success(await treatmentProcedureService.ToDtoAsync(procedure, request.DoctorId, cancellationToken));
        }
    }
}
