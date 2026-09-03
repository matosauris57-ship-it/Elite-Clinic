namespace Clinic_System.Application.Features.DentalTreatments.Queries.Handlers
{
    public class GetDentalTreatmentsAdminListQueryHandler : AppRequestHandler<GetDentalTreatmentsAdminListQuery, DentalTreatmentsAdminPageDTO>
    {
        private readonly IDentalTreatmentService dentalTreatmentService;
        private readonly IMapper mapper;

        public GetDentalTreatmentsAdminListQueryHandler(
            ICurrentUserService currentUserService,
            IDentalTreatmentService dentalTreatmentService,
            IMapper mapper) : base(currentUserService)
        {
            this.dentalTreatmentService = dentalTreatmentService;
            this.mapper = mapper;
        }

        public override async Task<Response<DentalTreatmentsAdminPageDTO>> Handle(GetDentalTreatmentsAdminListQuery request, CancellationToken cancellationToken)
        {
            IReadOnlyCollection<DentalTreatmentStatus>? statuses = null;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (string.Equals(request.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    statuses = [DentalTreatmentStatus.Planned, DentalTreatmentStatus.InProgress];
                }
                else if (Enum.TryParse<DentalTreatmentStatus>(request.Status, true, out var parsed))
                {
                    statuses = [parsed];
                }
            }

            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.IsPaged ? request.PageSize!.Value : 0;

            var (items, totalCount, statusCounts) = await dentalTreatmentService.GetAllForAdminAsync(
                request.Search,
                statuses,
                request.FromDate,
                request.ToDate,
                pageNumber,
                pageSize,
                cancellationToken);

            var resultPageSize = pageSize > 0 ? pageSize : Math.Max(totalCount, 1);
            var totalPages = (int)Math.Ceiling(totalCount / (double)resultPageSize);

            return Success(new DentalTreatmentsAdminPageDTO
            {
                Items = mapper.Map<List<DentalTreatmentListItemDTO>>(items),
                CurrentPage = pageNumber,
                PageSize = resultPageSize,
                TotalCount = totalCount,
                TotalPages = Math.Max(totalPages, 1),
                PlannedCount = Count(statusCounts, DentalTreatmentStatus.Planned),
                InProgressCount = Count(statusCounts, DentalTreatmentStatus.InProgress),
                CompletedCount = Count(statusCounts, DentalTreatmentStatus.Completed),
                CancelledCount = Count(statusCounts, DentalTreatmentStatus.Cancelled)
            });
        }

        private static int Count(IReadOnlyDictionary<DentalTreatmentStatus, int> counts, DentalTreatmentStatus status)
            => counts.TryGetValue(status, out var value) ? value : 0;
    }
}
