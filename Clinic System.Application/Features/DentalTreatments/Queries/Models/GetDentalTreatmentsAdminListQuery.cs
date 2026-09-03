namespace Clinic_System.Application.Features.DentalTreatments.Queries.Models
{
    public class GetDentalTreatmentsAdminListQuery : IRequest<Response<DentalTreatmentsAdminPageDTO>>
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int? PageSize { get; set; }

        public bool IsPaged => PageSize is > 0;
    }
}
