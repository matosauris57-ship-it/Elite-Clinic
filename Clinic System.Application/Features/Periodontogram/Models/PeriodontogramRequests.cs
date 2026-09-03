namespace Clinic_System.Application.Features.Periodontogram.Models;

public class ListPeriodontalExamsQuery : IRequest<Response<List<PeriodontalExamSummaryDTO>>>
{
    public int PatientId { get; set; }
}

public class GetPeriodontalExamQuery : IRequest<Response<PeriodontalExamDTO>>
{
    public int ExamId { get; set; }
}

public class ComparePeriodontalExamsQuery : IRequest<Response<PeriodontalCompareDTO>>
{
    public int PreviousExamId { get; set; }
    public int CurrentExamId { get; set; }
}

public class CreatePeriodontalExamCommand : IRequest<Response<PeriodontalExamDTO>>
{
    public int PatientId { get; set; }
    public bool CopyLatest { get; set; }
}

public class SavePeriodontalExamCommand : IRequest<Response<PeriodontalExamDTO>>
{
    public int ExamId { get; set; }
    public DateTime? ExaminedAt { get; set; }
    public string? Notes { get; set; }
    public List<PeriodontalToothDTO> Teeth { get; set; } = [];
}

public class DeletePeriodontalExamCommand : IRequest<Response<string>>
{
    public int ExamId { get; set; }
}
