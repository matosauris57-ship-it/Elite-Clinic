namespace Clinic_System.Application.Features.ToothRecords.Queries.Models
{
    public class GetToothRecordsByPatientQuery : IRequest<Response<List<ToothRecordDTO>>>
    {
        public int PatientId { get; set; }
    }
}
