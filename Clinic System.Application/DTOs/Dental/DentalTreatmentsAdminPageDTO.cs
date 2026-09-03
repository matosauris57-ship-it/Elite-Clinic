namespace Clinic_System.Application.DTOs.Dental
{
    public class DentalTreatmentsAdminPageDTO
    {
        public List<DentalTreatmentListItemDTO> Items { get; set; } = [];
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int PlannedCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
    }
}
