namespace Clinic_System.Application.Features.TreatmentProcedures.Queries.Handlers
{
    public class GetTreatmentProcedureListQueryHandler : AppRequestHandler<GetTreatmentProcedureListQuery, List<TreatmentProcedureDTO>>
    {
        private readonly ITreatmentProcedureService treatmentProcedureService;

        public GetTreatmentProcedureListQueryHandler(
            ICurrentUserService currentUserService,
            ITreatmentProcedureService treatmentProcedureService) : base(currentUserService)
        {
            this.treatmentProcedureService = treatmentProcedureService;
        }

        public override async Task<Response<List<TreatmentProcedureDTO>>> Handle(GetTreatmentProcedureListQuery request, CancellationToken cancellationToken)
        {
            var procedures = await treatmentProcedureService.GetAllAsync(request.ActiveOnly, cancellationToken);
            return Success(await treatmentProcedureService.ToDtosAsync(procedures, request.DoctorId, cancellationToken));
        }
    }
}
