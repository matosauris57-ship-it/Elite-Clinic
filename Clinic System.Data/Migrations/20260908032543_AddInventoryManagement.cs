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

            var seedDate = new DateTime(2026, 9, 8, 0, 0, 0, DateTimeKind.Utc);

            migrationBuilder.InsertData(
                table: "TreatmentProcedures",
                columns: new[] { "Id", "Code", "Category", "Name", "Price", "DurationMinutes", "IsActive", "IsDeleted", "CreatedAt" },
                values: new object[] { 13, "restauracion-resina", "RESTAURATIVA", "Restauración con resina", 2500m, 45, true, false, seedDate });

            migrationBuilder.InsertData(
                table: "InventoryItems",
                columns: new[] { "Id", "Sku", "Name", "Category", "Unit", "QuantityOnHand", "MinimumStock", "IsActive", "IsDeleted", "CreatedAt" },
                values: new object[,]
                {
                    { 1, "resina-a2", "Resina A2", "MATERIALES", "u", 50m, 10m, true, false, seedDate },
                    { 2, "anestesia", "Anestesia", "MEDICAMENTOS", "u", 100m, 20m, true, false, seedDate },
                    { 3, "aguja", "Aguja", "CONSUMIBLES", "u", 200m, 50m, true, false, seedDate },
                    { 4, "guantes", "Guantes", "CONSUMIBLES", "u", 500m, 100m, true, false, seedDate },
                    { 5, "microbrush", "Microbrush", "CONSUMIBLES", "u", 300m, 50m, true, false, seedDate },
                    { 6, "acido-grabador", "Ácido grabador", "MATERIALES", "ml", 50m, 10m, true, false, seedDate },
                    { 7, "adhesivo", "Adhesivo", "MATERIALES", "ml", 40m, 8m, true, false, seedDate },
                    { 8, "pasta-profilaxis", "Pasta profilaxis", "MATERIALES", "g", 100m, 20m, true, false, seedDate }
                });

            migrationBuilder.InsertData(
                table: "ProcedureMaterials",
                columns: new[] { "Id", "TreatmentProcedureId", "InventoryItemId", "DefaultQuantity", "IsOptional", "SortOrder", "CreatedAt" },
                values: new object[,]
                {
                    { 1, 13, 1, 1m, false, 1, seedDate },
                    { 2, 13, 2, 1m, false, 2, seedDate },
                    { 3, 13, 3, 1m, false, 3, seedDate },
                    { 4, 13, 4, 1m, false, 4, seedDate },
                    { 5, 13, 5, 2m, false, 5, seedDate },
                    { 6, 13, 6, 0.2m, false, 6, seedDate },
                    { 7, 13, 7, 0.1m, false, 7, seedDate },
                    { 8, 1, 4, 1m, false, 1, seedDate },
                    { 9, 1, 8, 2m, false, 2, seedDate }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM [ProcedureMaterials] WHERE [Id] BETWEEN 1 AND 9");
            migrationBuilder.Sql("DELETE FROM [InventoryItems] WHERE [Id] BETWEEN 1 AND 8");
            migrationBuilder.Sql("DELETE FROM [TreatmentProcedures] WHERE [Id] = 13");

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
