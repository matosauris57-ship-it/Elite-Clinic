using AutoMapper;
using Clinic_System.Application.DTOs.Dental;
using Clinic_System.Application.Mapping.Dental;

namespace Clinic_System.Application.Tests.Service.Implemention;

public class TreatmentProcedureServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ITreatmentProcedureRepository> _procedures = new();
    private readonly Mock<IDoctorProcedurePriceRepository> _doctorPrices = new();
    private readonly Mock<IDoctorRepository> _doctors = new();
    private readonly TreatmentProcedureService _sut;

    public TreatmentProcedureServiceTests()
    {
        _uow.SetupGet(x => x.TreatmentProceduresRepository).Returns(_procedures.Object);
        _uow.SetupGet(x => x.DoctorProcedurePricesRepository).Returns(_doctorPrices.Object);
        _uow.SetupGet(x => x.DoctorsRepository).Returns(_doctors.Object);

        var mapper = new MapperConfiguration(cfg => cfg.AddProfile<DentalProfile>()).CreateMapper();
        _sut = new TreatmentProcedureService(_uow.Object, mapper);
    }

    [Fact]
    public async Task CreateAsync_FixedMode_PersistsCatalogPrice()
    {
        _procedures.Setup(r => r.GetByCodeAsync("limp", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TreatmentProcedure?)null);
        TreatmentProcedure? saved = null;
        _procedures.Setup(r => r.AddAsync(It.IsAny<TreatmentProcedure>(), It.IsAny<CancellationToken>()))
            .Callback<TreatmentProcedure, CancellationToken>((p, _) => saved = p)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(
            "LIMP", "PREVENTIVO", "Limpieza", 1500m,
            TreatmentPricingMode.Fixed, 30, true, CancellationToken.None);

        result.PricingMode.Should().Be(TreatmentPricingMode.Fixed);
        result.Price.Should().Be(1500m);
        saved!.Price.Should().Be(1500m);
    }

    [Fact]
    public async Task CreateAsync_AtBillingMode_StoresZeroCatalogPrice()
    {
        _procedures.Setup(r => r.GetByCodeAsync("orto", It.IsAny<CancellationToken>()))
            .ReturnsAsync((TreatmentProcedure?)null);
        _procedures.Setup(r => r.AddAsync(It.IsAny<TreatmentProcedure>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(
            "ORTO", "ORTODONCIA", "Ajuste", 999m,
            TreatmentPricingMode.AtBilling, 45, true, CancellationToken.None);

        result.PricingMode.Should().Be(TreatmentPricingMode.AtBilling);
        result.Price.Should().Be(0m);
    }

    [Fact]
    public async Task ToDtosAsync_AtBooking_ShowsModeInsteadOfPrice()
    {
        var procedure = new TreatmentProcedure
        {
            Id = 3,
            Code = "eval",
            Category = "DIAG",
            Name = "Evaluación",
            Price = 0,
            PricingMode = TreatmentPricingMode.AtBooking,
            DurationMinutes = 20,
            IsActive = true
        };

        _doctorPrices.Setup(r => r.GetByProcedureIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        _doctors.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Doctor>());

        var dtos = await _sut.ToDtosAsync([procedure], null, CancellationToken.None);

        dtos.Should().ContainSingle();
        dtos[0].PricingMode.Should().Be("AtBooking");
        dtos[0].PricingModeDisplay.Should().Be("Al agendar");
        dtos[0].PriceDisplay.Should().Be("Al agendar");
        dtos[0].Price.Should().Be(0m);
    }
}
