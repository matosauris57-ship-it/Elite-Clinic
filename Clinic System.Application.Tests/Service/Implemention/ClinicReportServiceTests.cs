using Clinic_System.Application.DTOs.Reports;
using Clinic_System.Application.Service.Implemention;
using Clinic_System.Core.Interfaces.Repositories.IEntitiesRepository;
using Clinic_System.Core.Interfaces.UnitOfWork;
using Clinic_System.Core.Reports;
using FluentValidation;
using Moq;

namespace Clinic_System.Application.Tests.Service.Implemention;

public class ClinicReportServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IClinicReportRepository> _reports = new();
    private readonly ClinicReportService _sut;

    public ClinicReportServiceTests()
    {
        _uow.SetupGet(x => x.ClinicReportsRepository).Returns(_reports.Object);
        _sut = new ClinicReportService(_uow.Object);
    }

    [Fact]
    public async Task GetReportsAsync_MapsSnapshot_AndFormatsMoney()
    {
        _reports.Setup(r => r.GetSnapshotAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClinicReportSnapshot
            {
                FromDate = new DateTime(2026, 9, 1),
                ToDate = new DateTime(2026, 9, 8),
                Attendance = new AttendanceSnapshot
                {
                    Total = 10,
                    Completed = 6,
                    Confirmed = 2,
                    NoShow = 1,
                    Cancelled = 1
                },
                Revenue = new RevenueSnapshot
                {
                    Total = 1500,
                    Cash = 500,
                    Card = 700,
                    InstaPay = 300,
                    TransactionCount = 5,
                    OutstandingBalance = 200
                },
                Patients = new PatientSnapshot
                {
                    TotalPatients = 40,
                    NewInPeriod = 3,
                    ReturningInPeriod = 8
                }
            });

        var result = await _sut.GetReportsAsync(new DateTime(2026, 9, 1), new DateTime(2026, 9, 8));

        result.Attendance.Total.Should().Be(10);
        result.Attendance.AttendanceRate.Should().Be(80.0m);
        result.Attendance.NoShowRate.Should().Be(10.0m);
        result.Revenue.Total.Should().Be(1500);
        result.Revenue.TotalDisplay.Should().NotBeNullOrWhiteSpace();
        result.Patients.NewInPeriod.Should().Be(3);
        result.FromDate.Should().Be("2026-09-01");
        result.ToDate.Should().Be("2026-09-08");
    }

    [Fact]
    public async Task GetReportsAsync_RejectsInvertedRange()
    {
        var act = () => _sut.GetReportsAsync(new DateTime(2026, 9, 10), new DateTime(2026, 9, 1));
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*hasta*");
    }

    [Fact]
    public async Task GetReportsAsync_RejectsRangeOverOneYear()
    {
        var act = () => _sut.GetReportsAsync(new DateTime(2025, 1, 1), new DateTime(2026, 2, 1));
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*12 meses*");
    }
}
