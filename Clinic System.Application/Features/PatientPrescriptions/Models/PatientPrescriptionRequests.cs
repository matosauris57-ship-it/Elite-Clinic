namespace Clinic_System.Application.Features.PatientPrescriptions.Models;

public class ListPrescriptionTemplatesQuery : IRequest<Response<List<PrescriptionTemplateDTO>>>
{
}

public class ListPatientPrescriptionsQuery : IRequest<Response<List<PatientPrescriptionSummaryDTO>>>
{
    public int PatientId { get; set; }
}

public class GetPatientPrescriptionQuery : IRequest<Response<PatientPrescriptionDTO>>
{
    public int PrescriptionId { get; set; }
}

public class CreatePatientPrescriptionCommand : IRequest<Response<PatientPrescriptionDTO>>
{
    public int PatientId { get; set; }
    public int? DoctorId { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public List<string> TemplateKeys { get; set; } = [];
    public List<PatientPrescriptionItemDTO> Items { get; set; } = [];
}

public class UpdatePatientPrescriptionCommand : IRequest<Response<PatientPrescriptionDTO>>
{
    public int PrescriptionId { get; set; }
    public int? DoctorId { get; set; }
    public DateTime? IssuedAt { get; set; }
    public string? Diagnosis { get; set; }
    public string? Notes { get; set; }
    public List<string> TemplateKeys { get; set; } = [];
    public List<PatientPrescriptionItemDTO> Items { get; set; } = [];
}

public class DeletePatientPrescriptionCommand : IRequest<Response<string>>
{
    public int PrescriptionId { get; set; }
}
