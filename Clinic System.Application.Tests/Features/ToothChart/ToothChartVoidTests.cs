namespace Clinic_System.Application.Tests.Features.ToothChart;

public class ToothChartVoidTests
{
    [Fact]
    public void Void_Twice_Throws()
    {
        var entry = new ToothChartEntry();

        entry.Void("user-1");

        entry.IsVoided.Should().BeTrue();
        ((Action)(() => entry.Void("user-2"))).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task VoidEntry_RestoresPreviousSummaryAndMarksTimeline()
    {
        var current = new ToothChartEntry
        {
            Id = 12,
            PatientId = 7,
            ToothNumber = 26,
            Surface = ToothSurface.WholeTooth,
            Phase = ToothChartPhase.Diagnosis,
            Condition = ToothCondition.Caries,
            Notes = "actual",
            RecordedAt = DateTime.UtcNow
        };
        var previous = new ToothChartEntry
        {
            Id = 8,
            PatientId = 7,
            ToothNumber = 26,
            Phase = ToothChartPhase.Diagnosis,
            Condition = ToothCondition.Healthy,
            Notes = "sano",
            RecordedAt = DateTime.UtcNow.AddDays(-1)
        };
        var linked = new DentalClinicalEvent
        {
            Id = 3,
            PatientId = 7,
            ToothNumber = 26,
            Type = DentalClinicalEventType.OdontogramEntry,
            ReferenceId = "12",
            RecordedAt = current.RecordedAt
        };
        var summary = new ToothRecord
        {
            PatientId = 7,
            ToothNumber = 26,
            DiagnosisCondition = ToothCondition.Caries,
            Notes = "actual"
        };

        var entries = new Mock<IToothChartEntryRepository>();
        entries.Setup(x => x.GetByCondition(It.IsAny<Expression<Func<ToothChartEntry, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(current);
        entries.Setup(x => x.FindAsync(It.IsAny<Expression<Func<ToothChartEntry, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([previous]);

        var events = new Mock<IDentalClinicalEventRepository>();
        events.Setup(x => x.GetByCondition(It.IsAny<Expression<Func<DentalClinicalEvent, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(linked);

        var records = new Mock<IToothRecordRepository>();
        records.Setup(x => x.GetByPatientAndToothAsync(7, 26, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(x => x.ToothChartEntriesRepository).Returns(entries.Object);
        unitOfWork.SetupGet(x => x.DentalClinicalEventsRepository).Returns(events.Object);
        unitOfWork.SetupGet(x => x.ToothRecordsRepository).Returns(records.Object);

        var service = new ToothChartService(unitOfWork.Object);
        await service.VoidEntryAsync(12, "user-1", null, CancellationToken.None);

        current.IsVoided.Should().BeTrue();
        linked.IsVoided.Should().BeTrue();
        summary.DiagnosisCondition.Should().Be(ToothCondition.Healthy);
        summary.Notes.Should().Be("sano");
        entries.Verify(x => x.Update(current, It.IsAny<CancellationToken>()), Times.Once);
        events.Verify(x => x.Update(linked, It.IsAny<CancellationToken>()), Times.Once);
        records.Verify(x => x.Update(summary, It.IsAny<CancellationToken>()), Times.Once);
    }
}
