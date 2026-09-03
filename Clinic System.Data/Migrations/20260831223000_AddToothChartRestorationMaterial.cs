using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260831223000_AddToothChartRestorationMaterial")]
    public class AddToothChartRestorationMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RestorationMaterial",
                table: "ToothChartEntries",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RestorationMaterial",
                table: "ToothChartEntries");
        }
    }
}
