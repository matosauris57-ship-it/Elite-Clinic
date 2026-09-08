using Clinic_System.Application.DTOs.Inventory;

namespace Clinic_System.Application.Tests.Service.Implemention;

public class InventoryServiceTests
{
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IInventoryItemRepository> _items = new();
    private readonly Mock<IStockMovementRepository> _movements = new();
    private readonly Mock<IProcedureMaterialRepository> _bom = new();
    private readonly Mock<ITreatmentMaterialConsumptionRepository> _consumptions = new();
    private readonly Mock<IDentalTreatmentRepository> _treatments = new();
    private readonly Mock<ITreatmentProcedureRepository> _procedures = new();
    private readonly InventoryService _sut;

    public InventoryServiceTests()
    {
        _uow.SetupGet(x => x.InventoryItemsRepository).Returns(_items.Object);
        _uow.SetupGet(x => x.StockMovementsRepository).Returns(_movements.Object);
        _uow.SetupGet(x => x.ProcedureMaterialsRepository).Returns(_bom.Object);
        _uow.SetupGet(x => x.TreatmentMaterialConsumptionsRepository).Returns(_consumptions.Object);
        _uow.SetupGet(x => x.DentalTreatmentsRepository).Returns(_treatments.Object);
        _uow.SetupGet(x => x.TreatmentProceduresRepository).Returns(_procedures.Object);
        _uow.Setup(x => x.SaveAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        _sut = new InventoryService(_uow.Object);
    }

    [Fact]
    public async Task ProposeConsumption_ReturnsBomLines_ForProcedureWithMaterials()
    {
        var treatment = new DentalTreatment
        {
            Id = 10,
            ProcedureName = "Restauración con resina",
            TreatmentProcedureId = 13,
            Status = DentalTreatmentStatus.InProgress
        };
        _treatments.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(treatment);
        _consumptions.Setup(r => r.GetByTreatmentIdWithLinesAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TreatmentMaterialConsumption?)null);
        _bom.Setup(r => r.GetByProcedureIdWithItemsAsync(13, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ProcedureMaterial
                {
                    InventoryItemId = 1,
                    DefaultQuantity = 1,
                    IsOptional = false,
                    InventoryItem = new InventoryItem
                    {
                        Id = 1, Sku = "resina-a2", Name = "Resina A2", Unit = "u", QuantityOnHand = 50
                    }
                },
                new ProcedureMaterial
                {
                    InventoryItemId = 5,
                    DefaultQuantity = 2,
                    IsOptional = false,
                    InventoryItem = new InventoryItem
                    {
                        Id = 5, Sku = "microbrush", Name = "Microbrush", Unit = "u", QuantityOnHand = 1
                    }
                }
            ]);

        var proposal = await _sut.ProposeConsumptionAsync(10);

        proposal.HasBom.Should().BeTrue();
        proposal.Lines.Should().HaveCount(2);
        proposal.Lines[0].Name.Should().Be("Resina A2");
        proposal.Lines[0].ActualQuantity.Should().Be(1);
        proposal.Lines[0].IsIncluded.Should().BeTrue();
        proposal.Lines[1].InsufficientStock.Should().BeTrue();
    }

    [Fact]
    public async Task ConfirmConsumption_DeductsStock_AndCreatesOutMovements()
    {
        var treatment = new DentalTreatment
        {
            Id = 10,
            ProcedureName = "Restauración con resina",
            TreatmentProcedureId = 13
        };
        var item = new InventoryItem
        {
            Id = 1, Sku = "resina-a2", Name = "Resina A2", Unit = "u", QuantityOnHand = 10
        };

        _consumptions.Setup(r => r.GetByTreatmentIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TreatmentMaterialConsumption?)null);
        _treatments.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(treatment);
        _bom.Setup(r => r.GetByProcedureIdAsync(13, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new ProcedureMaterial { InventoryItemId = 1, DefaultQuantity = 1 }]);
        _items.Setup(r => r.GetByIdForUpdateAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _movements.Setup(r => r.AddAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _consumptions.Setup(r => r.AddAsync(It.IsAny<TreatmentMaterialConsumption>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.ConfirmConsumptionAsync(
            10,
            [new MaterialConsumptionLineInput { InventoryItemId = 1, Quantity = 1, IsIncluded = true, ProposedQuantity = 1 }],
            null,
            "user-1");

        item.QuantityOnHand.Should().Be(9);
        result.Lines.Should().ContainSingle(l => l.IsIncluded && l.ActualQuantity == 1);
        _movements.Verify(r => r.AddAsync(
            It.Is<StockMovement>(m => m.Type == StockMovementType.Out && m.Quantity == 1 && m.ReferenceType == nameof(DentalTreatment)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterEntry_IncreasesOnHand()
    {
        var item = new InventoryItem { Id = 4, Name = "Guantes", Unit = "u", QuantityOnHand = 100 };
        _items.Setup(r => r.GetByIdForUpdateAsync(4, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        _movements.Setup(r => r.AddAsync(It.IsAny<StockMovement>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var movement = await _sut.RegisterEntryAsync(4, 25, "Compra", null, "admin");

        item.QuantityOnHand.Should().Be(125);
        movement.Type.Should().Be(StockMovementType.In);
        movement.QuantityBefore.Should().Be(100);
        movement.QuantityAfter.Should().Be(125);
    }
}
