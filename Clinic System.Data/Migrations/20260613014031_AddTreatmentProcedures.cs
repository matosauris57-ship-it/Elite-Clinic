using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTreatmentProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TreatmentProcedures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationMinutes = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentProcedures", x => x.Id);
                    table.CheckConstraint("CK_TreatmentProcedures_Duration", "[DurationMinutes] > 0");
                    table.CheckConstraint("CK_TreatmentProcedures_Price", "[Price] >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentProcedures_Code",
                table: "TreatmentProcedures",
                column: "Code",
                unique: true);

            var seedDate = new DateTime(2026, 6, 13, 0, 0, 0, DateTimeKind.Utc);
            migrationBuilder.InsertData(
                table: "TreatmentProcedures",
                columns: new[] { "Id", "Code", "Category", "Name", "Price", "DurationMinutes", "IsActive", "IsDeleted", "CreatedAt" },
                values: new object[,]
                {
                    { 1, "limpieza", "PREVENTIVO", "Limpieza dental", 850m, 45, true, false, seedDate },
                    { 2, "extraccion-simple", "CIRUGÍA", "Extracción simple", 1200m, 30, true, false, seedDate },
                    { 3, "extraccion-molar", "CIRUGÍA", "Extracción molar", 2500m, 60, true, false, seedDate },
                    { 4, "revision-ortodoncia", "ORTODONCIA", "Revisión ortodoncia", 600m, 30, true, false, seedDate },
                    { 5, "brackets", "ORTODONCIA", "Colocación brackets", 3500m, 90, true, false, seedDate },
                    { 6, "blanqueamiento", "ESTÉTICA", "Blanqueamiento", 2800m, 60, true, false, seedDate },
                    { 7, "corona", "ESTÉTICA", "Corona de porcelana", 7500m, 90, true, false, seedDate },
                    { 8, "endodoncia", "ENDODONCIA", "Endodoncia", 4200m, 75, true, false, seedDate },
                    { 9, "implante", "IMPLANTOLOGÍA", "Implante dental", 15000m, 120, true, false, seedDate },
                    { 10, "radiografia", "DIAGNÓSTICO", "Radiografía panorámica", 450m, 15, true, false, seedDate },
                    { 11, "revision", "PREVENTIVO", "Revisión + Diagnóstico", 500m, 30, true, false, seedDate },
                    { 12, "sellantes", "PREVENTIVO", "Sellantes", 350m, 20, true, false, seedDate }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TreatmentProcedures");
        }
    }
}
