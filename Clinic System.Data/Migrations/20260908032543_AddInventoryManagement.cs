using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Sku = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    QuantityOnHand = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    MinimumStock = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                    table.CheckConstraint("CK_InventoryItems_MinimumStock", "[MinimumStock] >= 0");
                    table.CheckConstraint("CK_InventoryItems_QuantityOnHand", "[QuantityOnHand] >= 0");
                });

            migrationBuilder.CreateTable(
                name: "TreatmentMaterialConsumptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DentalTreatmentId = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ConfirmedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentMaterialConsumptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TreatmentMaterialConsumptions_DentalTreatments_DentalTreatmentId",
                        column: x => x.DentalTreatmentId,
                        principalTable: "DentalTreatments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProcedureMaterials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TreatmentProcedureId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    DefaultQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcedureMaterials", x => x.Id);
                    table.CheckConstraint("CK_ProcedureMaterials_DefaultQuantity", "[DefaultQuantity] > 0");
                    table.ForeignKey(
                        name: "FK_ProcedureMaterials_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProcedureMaterials_TreatmentProcedures_TreatmentProcedureId",
                        column: x => x.TreatmentProcedureId,
                        principalTable: "TreatmentProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StockMovements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    QuantityBefore = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    QuantityAfter = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    ReversesMovementId = table.Column<int>(type: "int", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovements", x => x.Id);
                    table.CheckConstraint("CK_StockMovements_Quantity", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_StockMovements_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockMovements_StockMovements_ReversesMovementId",
                        column: x => x.ReversesMovementId,
                        principalTable: "StockMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentMaterialConsumptionLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsumptionId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    ProposedQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,3)", nullable: false),
                    IsIncluded = table.Column<bool>(type: "bit", nullable: false),
                    StockMovementId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentMaterialConsumptionLines", x => x.Id);
                    table.CheckConstraint("CK_TreatmentMaterialConsumptionLines_Actual", "[ActualQuantity] >= 0");
                    table.CheckConstraint("CK_TreatmentMaterialConsumptionLines_Proposed", "[ProposedQuantity] >= 0");
                    table.ForeignKey(
                        name: "FK_TreatmentMaterialConsumptionLines_InventoryItems_InventoryItemId",
                        column: x => x.InventoryItemId,
                        principalTable: "InventoryItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TreatmentMaterialConsumptionLines_StockMovements_StockMovementId",
                        column: x => x.StockMovementId,
                        principalTable: "StockMovements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TreatmentMaterialConsumptionLines_TreatmentMaterialConsumptions_ConsumptionId",
                        column: x => x.ConsumptionId,
                        principalTable: "TreatmentMaterialConsumptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_Sku",
                table: "InventoryItems",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureMaterials_InventoryItemId",
                table: "ProcedureMaterials",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcedureMaterials_TreatmentProcedureId_InventoryItemId",
                table: "ProcedureMaterials",
                columns: new[] { "TreatmentProcedureId", "InventoryItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_InventoryItemId",
                table: "StockMovements",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ReferenceType_ReferenceId",
                table: "StockMovements",
                columns: new[] { "ReferenceType", "ReferenceId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ReversesMovementId",
                table: "StockMovements",
                column: "ReversesMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentMaterialConsumptionLines_ConsumptionId_InventoryItemId",
                table: "TreatmentMaterialConsumptionLines",
                columns: new[] { "ConsumptionId", "InventoryItemId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentMaterialConsumptionLines_InventoryItemId",
                table: "TreatmentMaterialConsumptionLines",
                column: "InventoryItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentMaterialConsumptionLines_StockMovementId",
                table: "TreatmentMaterialConsumptionLines",
                column: "StockMovementId");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentMaterialConsumptions_DentalTreatmentId",
                table: "TreatmentMaterialConsumptions",
                column: "DentalTreatmentId",
                unique: true);

            // Seed is idempotent: local DBs may already have Id 13 used by another procedure.
            migrationBuilder.Sql("""
                DECLARE @seedDate datetime2 = '2026-09-08T00:00:00.0000000Z';

                IF NOT EXISTS (SELECT 1 FROM [TreatmentProcedures] WHERE [Code] = N'restauracion-resina')
                BEGIN
                    INSERT INTO [TreatmentProcedures]
                        ([Code], [Category], [Name], [Price], [DurationMinutes], [IsActive], [IsDeleted], [CreatedAt])
                    VALUES
                        (N'restauracion-resina', N'RESTAURATIVA', N'Restauración con resina', 2500.0, 45, CAST(1 AS bit), CAST(0 AS bit), @seedDate);
                END

                DECLARE @resinaProcId int =
                    (SELECT TOP (1) [Id] FROM [TreatmentProcedures] WHERE [Code] = N'restauracion-resina' ORDER BY [Id]);

                IF NOT EXISTS (SELECT 1 FROM [InventoryItems] WHERE [Sku] = N'resina-a2')
                    INSERT INTO [InventoryItems]
                        ([Sku], [Name], [Category], [Unit], [QuantityOnHand], [MinimumStock], [IsActive], [IsDeleted], [CreatedAt])
                    VALUES (N'resina-a2', N'Resina A2', N'MATERIALES', N'u', 50, 10, 1, 0, @seedDate);
                IF NOT EXISTS (SELECT 1 FROM [InventoryItems] WHERE [Sku] = N'anestesia')
                    INSERT INTO [InventoryItems]
                        ([Sku], [Name], [Category], [Unit], [QuantityOnHand], [MinimumStock], [IsActive], [IsDeleted], [CreatedAt])
                    VALUES (N'anestesia', N'Anestesia', N'MEDICAMENTOS', N'u', 100, 20, 1, 0, @seedDate);
                IF NOT EXISTS (SELECT 1 FROM [InventoryItems] WHERE [Sku] = N'aguja')
                    INSERT INTO [InventoryItems]
                        ([Sku], [Name], [Category], [Unit], [QuantityOnHand], [MinimumStock], [IsActive], [IsDeleted], [CreatedAt])
                    VALUES (N'aguja', N'Aguja', N'CONSUMIBLES', N'u', 200, 50, 1, 0, @seedDate);
                IF NOT EXISTS (SELECT 1 FROM [InventoryItems] WHERE [Sku] = N'guantes')
                    INSERT INTO [InventoryItems]
                        ([Sku], [Name], [Category], [Unit], [QuantityOnHand], [MinimumStock], [IsActive], [IsDeleted], [CreatedAt])
                    VALUES (N'guantes', N'Guantes', N'CONSUMIBLES', N'u', 500, 100, 1, 0, @seedDate);
                IF NOT EXISTS (SELECT 1 FROM [InventoryItems] WHERE [Sku] = N'microbrush')
                    INSERT INTO [InventoryItems]
                        ([Sku], [Name], [Category], [Unit], [QuantityOnHand], [MinimumStock], [IsActive], [IsDeleted], [CreatedAt])
                    VALUES (N'microbrush', N'Microbrush', N'CONSUMIBLES', N'u', 300, 50, 1, 0, @seedDate);
                IF NOT EXISTS (SELECT 1 FROM [InventoryItems] WHERE [Sku] = N'acido-grabador')
                    INSERT INTO [InventoryItems]
                        ([Sku], [Name], [Category], [Unit], [QuantityOnHand], [MinimumStock], [IsActive], [IsDeleted], [CreatedAt])
                    VALUES (N'acido-grabador', N'Ácido grabador', N'MATERIALES', N'ml', 50, 10, 1, 0, @seedDate);
                IF NOT EXISTS (SELECT 1 FROM [InventoryItems] WHERE [Sku] = N'adhesivo')
                    INSERT INTO [InventoryItems]
                        ([Sku], [Name], [Category], [Unit], [QuantityOnHand], [MinimumStock], [IsActive], [IsDeleted], [CreatedAt])
                    VALUES (N'adhesivo', N'Adhesivo', N'MATERIALES', N'ml', 40, 8, 1, 0, @seedDate);
                IF NOT EXISTS (SELECT 1 FROM [InventoryItems] WHERE [Sku] = N'pasta-profilaxis')
                    INSERT INTO [InventoryItems]
                        ([Sku], [Name], [Category], [Unit], [QuantityOnHand], [MinimumStock], [IsActive], [IsDeleted], [CreatedAt])
                    VALUES (N'pasta-profilaxis', N'Pasta profilaxis', N'MATERIALES', N'g', 100, 20, 1, 0, @seedDate);

                DECLARE @itemResina int = (SELECT [Id] FROM [InventoryItems] WHERE [Sku] = N'resina-a2');
                DECLARE @itemAnestesia int = (SELECT [Id] FROM [InventoryItems] WHERE [Sku] = N'anestesia');
                DECLARE @itemAguja int = (SELECT [Id] FROM [InventoryItems] WHERE [Sku] = N'aguja');
                DECLARE @itemGuantes int = (SELECT [Id] FROM [InventoryItems] WHERE [Sku] = N'guantes');
                DECLARE @itemMicrobrush int = (SELECT [Id] FROM [InventoryItems] WHERE [Sku] = N'microbrush');
                DECLARE @itemAcido int = (SELECT [Id] FROM [InventoryItems] WHERE [Sku] = N'acido-grabador');
                DECLARE @itemAdhesivo int = (SELECT [Id] FROM [InventoryItems] WHERE [Sku] = N'adhesivo');
                DECLARE @itemPasta int = (SELECT [Id] FROM [InventoryItems] WHERE [Sku] = N'pasta-profilaxis');
                DECLARE @profilaxisProcId int =
                    (SELECT TOP (1) [Id] FROM [TreatmentProcedures] WHERE [Id] = 1 OR [Code] LIKE N'%profil%' ORDER BY CASE WHEN [Id] = 1 THEN 0 ELSE 1 END, [Id]);

                IF @resinaProcId IS NOT NULL AND @itemResina IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM [ProcedureMaterials] WHERE [TreatmentProcedureId] = @resinaProcId AND [InventoryItemId] = @itemResina)
                    INSERT INTO [ProcedureMaterials] ([TreatmentProcedureId], [InventoryItemId], [DefaultQuantity], [IsOptional], [SortOrder], [CreatedAt])
                    VALUES (@resinaProcId, @itemResina, 1, 0, 1, @seedDate);
                IF @resinaProcId IS NOT NULL AND @itemAnestesia IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM [ProcedureMaterials] WHERE [TreatmentProcedureId] = @resinaProcId AND [InventoryItemId] = @itemAnestesia)
                    INSERT INTO [ProcedureMaterials] ([TreatmentProcedureId], [InventoryItemId], [DefaultQuantity], [IsOptional], [SortOrder], [CreatedAt])
                    VALUES (@resinaProcId, @itemAnestesia, 1, 0, 2, @seedDate);
                IF @resinaProcId IS NOT NULL AND @itemAguja IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM [ProcedureMaterials] WHERE [TreatmentProcedureId] = @resinaProcId AND [InventoryItemId] = @itemAguja)
                    INSERT INTO [ProcedureMaterials] ([TreatmentProcedureId], [InventoryItemId], [DefaultQuantity], [IsOptional], [SortOrder], [CreatedAt])
                    VALUES (@resinaProcId, @itemAguja, 1, 0, 3, @seedDate);
                IF @resinaProcId IS NOT NULL AND @itemGuantes IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM [ProcedureMaterials] WHERE [TreatmentProcedureId] = @resinaProcId AND [InventoryItemId] = @itemGuantes)
                    INSERT INTO [ProcedureMaterials] ([TreatmentProcedureId], [InventoryItemId], [DefaultQuantity], [IsOptional], [SortOrder], [CreatedAt])
                    VALUES (@resinaProcId, @itemGuantes, 1, 0, 4, @seedDate);
                IF @resinaProcId IS NOT NULL AND @itemMicrobrush IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM [ProcedureMaterials] WHERE [TreatmentProcedureId] = @resinaProcId AND [InventoryItemId] = @itemMicrobrush)
                    INSERT INTO [ProcedureMaterials] ([TreatmentProcedureId], [InventoryItemId], [DefaultQuantity], [IsOptional], [SortOrder], [CreatedAt])
                    VALUES (@resinaProcId, @itemMicrobrush, 2, 0, 5, @seedDate);
                IF @resinaProcId IS NOT NULL AND @itemAcido IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM [ProcedureMaterials] WHERE [TreatmentProcedureId] = @resinaProcId AND [InventoryItemId] = @itemAcido)
                    INSERT INTO [ProcedureMaterials] ([TreatmentProcedureId], [InventoryItemId], [DefaultQuantity], [IsOptional], [SortOrder], [CreatedAt])
                    VALUES (@resinaProcId, @itemAcido, 0.2, 0, 6, @seedDate);
                IF @resinaProcId IS NOT NULL AND @itemAdhesivo IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM [ProcedureMaterials] WHERE [TreatmentProcedureId] = @resinaProcId AND [InventoryItemId] = @itemAdhesivo)
                    INSERT INTO [ProcedureMaterials] ([TreatmentProcedureId], [InventoryItemId], [DefaultQuantity], [IsOptional], [SortOrder], [CreatedAt])
                    VALUES (@resinaProcId, @itemAdhesivo, 0.1, 0, 7, @seedDate);

                IF @profilaxisProcId IS NOT NULL AND @itemGuantes IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM [ProcedureMaterials] WHERE [TreatmentProcedureId] = @profilaxisProcId AND [InventoryItemId] = @itemGuantes)
                    INSERT INTO [ProcedureMaterials] ([TreatmentProcedureId], [InventoryItemId], [DefaultQuantity], [IsOptional], [SortOrder], [CreatedAt])
                    VALUES (@profilaxisProcId, @itemGuantes, 1, 0, 1, @seedDate);
                IF @profilaxisProcId IS NOT NULL AND @itemPasta IS NOT NULL
                   AND NOT EXISTS (SELECT 1 FROM [ProcedureMaterials] WHERE [TreatmentProcedureId] = @profilaxisProcId AND [InventoryItemId] = @itemPasta)
                    INSERT INTO [ProcedureMaterials] ([TreatmentProcedureId], [InventoryItemId], [DefaultQuantity], [IsOptional], [SortOrder], [CreatedAt])
                    VALUES (@profilaxisProcId, @itemPasta, 2, 0, 2, @seedDate);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE pm
                FROM [ProcedureMaterials] pm
                INNER JOIN [InventoryItems] i ON i.[Id] = pm.[InventoryItemId]
                WHERE i.[Sku] IN (
                    N'resina-a2', N'anestesia', N'aguja', N'guantes',
                    N'microbrush', N'acido-grabador', N'adhesivo', N'pasta-profilaxis');

                DELETE FROM [InventoryItems]
                WHERE [Sku] IN (
                    N'resina-a2', N'anestesia', N'aguja', N'guantes',
                    N'microbrush', N'acido-grabador', N'adhesivo', N'pasta-profilaxis');

                DELETE FROM [TreatmentProcedures]
                WHERE [Code] = N'restauracion-resina'
                  AND [Name] = N'Restauración con resina';
                """);

            migrationBuilder.DropTable(
                name: "ProcedureMaterials");

            migrationBuilder.DropTable(
                name: "TreatmentMaterialConsumptionLines");

            migrationBuilder.DropTable(
                name: "StockMovements");

            migrationBuilder.DropTable(
                name: "TreatmentMaterialConsumptions");

            migrationBuilder.DropTable(
                name: "InventoryItems");
        }
    }
}
