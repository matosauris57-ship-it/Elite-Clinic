using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260908153000_AddTreatmentPricingMode")]
    public partial class AddTreatmentPricingMode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PricingMode",
                table: "TreatmentProcedures",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TreatmentProcedures_PricingMode",
                table: "TreatmentProcedures",
                sql: "[PricingMode] IN (0, 1, 2)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TreatmentProcedures_PricingMode",
                table: "TreatmentProcedures");

            migrationBuilder.DropColumn(
                name: "PricingMode",
                table: "TreatmentProcedures");
        }
    }
}
