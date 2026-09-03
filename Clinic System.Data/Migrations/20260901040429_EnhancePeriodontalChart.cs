using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class EnhancePeriodontalChart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PeriodontalTeeth",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FacialFurcation",
                table: "PeriodontalTeeth",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LingualFurcation",
                table: "PeriodontalTeeth",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KeratinizedGingivaMm",
                table: "PeriodontalTeeth",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [PeriodontalTeeth]
                SET [FacialFurcation] = [Furcation]
                WHERE [Furcation] <> 0;
                """);

            migrationBuilder.DropColumn(
                name: "Furcation",
                table: "PeriodontalTeeth");

            migrationBuilder.AddColumn<decimal>(
                name: "BleedingPercent",
                table: "PeriodontalExams",
                type: "decimal(5,1)",
                precision: 5,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MeanProbingDepthMm",
                table: "PeriodontalExams",
                type: "decimal(4,1)",
                precision: 4,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PlaquePercent",
                table: "PeriodontalExams",
                type: "decimal(5,1)",
                precision: 5,
                scale: 1,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecordedSiteCount",
                table: "PeriodontalExams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SitesDeepGe5",
                table: "PeriodontalExams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SitesDeepGe6",
                table: "PeriodontalExams",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_PeriodontalTeeth_KG",
                table: "PeriodontalTeeth",
                sql: "[KeratinizedGingivaMm] IS NULL OR ([KeratinizedGingivaMm] >= 0 AND [KeratinizedGingivaMm] <= 15)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_PeriodontalTeeth_KG",
                table: "PeriodontalTeeth");

            migrationBuilder.AddColumn<int>(
                name: "Furcation",
                table: "PeriodontalTeeth",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE [PeriodontalTeeth]
                SET [Furcation] = CASE
                    WHEN [FacialFurcation] >= [LingualFurcation] THEN [FacialFurcation]
                    ELSE [LingualFurcation]
                END;
                """);

            migrationBuilder.DropColumn(
                name: "FacialFurcation",
                table: "PeriodontalTeeth");

            migrationBuilder.DropColumn(
                name: "KeratinizedGingivaMm",
                table: "PeriodontalTeeth");

            migrationBuilder.DropColumn(
                name: "LingualFurcation",
                table: "PeriodontalTeeth");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PeriodontalTeeth");

            migrationBuilder.DropColumn(
                name: "BleedingPercent",
                table: "PeriodontalExams");

            migrationBuilder.DropColumn(
                name: "MeanProbingDepthMm",
                table: "PeriodontalExams");

            migrationBuilder.DropColumn(
                name: "PlaquePercent",
                table: "PeriodontalExams");

            migrationBuilder.DropColumn(
                name: "RecordedSiteCount",
                table: "PeriodontalExams");

            migrationBuilder.DropColumn(
                name: "SitesDeepGe5",
                table: "PeriodontalExams");

            migrationBuilder.DropColumn(
                name: "SitesDeepGe6",
                table: "PeriodontalExams");
        }
    }
}
