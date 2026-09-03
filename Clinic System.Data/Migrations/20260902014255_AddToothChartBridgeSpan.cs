using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddToothChartBridgeSpan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BridgeRole",
                table: "ToothChartEntries",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BridgeSpanId",
                table: "ToothChartEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToothChartEntries_BridgeSpanId",
                table: "ToothChartEntries",
                column: "BridgeSpanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ToothChartEntries_BridgeSpanId",
                table: "ToothChartEntries");

            migrationBuilder.DropColumn(
                name: "BridgeRole",
                table: "ToothChartEntries");

            migrationBuilder.DropColumn(
                name: "BridgeSpanId",
                table: "ToothChartEntries");
        }
    }
}
