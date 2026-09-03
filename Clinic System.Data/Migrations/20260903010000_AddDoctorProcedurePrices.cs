using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260903010000_AddDoctorProcedurePrices")]
    public partial class AddDoctorProcedurePrices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DoctorProcedurePrices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DoctorId = table.Column<int>(type: "int", nullable: false),
                    TreatmentProcedureId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorProcedurePrices", x => x.Id);
                    table.CheckConstraint("CK_DoctorProcedurePrices_Price", "[Price] >= 0");
                    table.ForeignKey(
                        name: "FK_DoctorProcedurePrices_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DoctorProcedurePrices_TreatmentProcedures_TreatmentProcedureId",
                        column: x => x.TreatmentProcedureId,
                        principalTable: "TreatmentProcedures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProcedurePrices_Doctor_Procedure",
                table: "DoctorProcedurePrices",
                columns: new[] { "DoctorId", "TreatmentProcedureId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DoctorProcedurePrices_TreatmentProcedureId",
                table: "DoctorProcedurePrices",
                column: "TreatmentProcedureId");

            migrationBuilder.Sql("""
                INSERT INTO DoctorProcedurePrices (DoctorId, TreatmentProcedureId, Price, CreatedAt)
                SELECT d.Id, p.Id, p.Price, SYSUTCDATETIME()
                FROM Doctors d
                CROSS JOIN TreatmentProcedures p
                WHERE d.IsDeleted = 0 AND p.IsDeleted = 0 AND p.Price > 0;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DoctorProcedurePrices");
        }
    }
}
