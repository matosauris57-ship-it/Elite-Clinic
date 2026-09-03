namespace Clinic_System.Application.Features.Appointments.Queries.Models
{
    public class GetAdminAgendaQuery : IRequest<Response<PagedResult<AppointmentAgendaItemDTO>>>
    {
        public DateTime? Date { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DoctorId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AppointmentStatus? Status { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int? PageSize { get; set; }

        public bool IsPaged => PageSize is > 0;
    }
}
