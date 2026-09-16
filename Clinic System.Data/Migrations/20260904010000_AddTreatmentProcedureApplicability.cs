using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260904010000_AddTreatmentProcedureApplicability")]
    public partial class AddTreatmentProcedureApplicability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Target",
                table: "TreatmentProcedures",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "ToothKindFilter",
                table: "TreatmentProcedures",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE TreatmentProcedures SET Target = 0, ToothKindFilter = 0
                WHERE Code IN (N'limpieza', N'revision', N'revision-ortodoncia', N'brackets', N'blanqueamiento', N'radiografia');

                UPDATE TreatmentProcedures SET Target = 2, ToothKindFilter = 0
                WHERE Code = N'sellantes';

                UPDATE TreatmentProcedures SET Target = 1, ToothKindFilter = 1
                WHERE Code = N'extraccion-molar';

                UPDATE TreatmentProcedures SET Target = 1, ToothKindFilter = 0
                WHERE Code IN (N'extraccion-simple', N'corona', N'endodoncia', N'implante');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Target", table: "TreatmentProcedures");
            migrationBuilder.DropColumn(name: "ToothKindFilter", table: "TreatmentProcedures");
        }
    }
}
