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
            // Idempotent: EnsureRequiredSchemaAsync may have already added PricingMode in local DBs.
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.TreatmentProcedures', N'PricingMode') IS NULL
                BEGIN
                    ALTER TABLE [TreatmentProcedures] ADD [PricingMode] int NOT NULL
                        CONSTRAINT [DF_TreatmentProcedures_PricingMode] DEFAULT (1);
                END

                IF NOT EXISTS (
                    SELECT 1 FROM sys.check_constraints
                    WHERE name = N'CK_TreatmentProcedures_PricingMode')
                BEGIN
                    ALTER TABLE [TreatmentProcedures] ADD CONSTRAINT [CK_TreatmentProcedures_PricingMode]
                        CHECK ([PricingMode] IN (0, 1, 2));
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM sys.check_constraints
                    WHERE name = N'CK_TreatmentProcedures_PricingMode')
                    ALTER TABLE [TreatmentProcedures] DROP CONSTRAINT [CK_TreatmentProcedures_PricingMode];

                IF OBJECT_ID(N'dbo.DF_TreatmentProcedures_PricingMode', N'D') IS NOT NULL
                    ALTER TABLE [TreatmentProcedures] DROP CONSTRAINT [DF_TreatmentProcedures_PricingMode];

                IF COL_LENGTH(N'dbo.TreatmentProcedures', N'PricingMode') IS NOT NULL
                    ALTER TABLE [TreatmentProcedures] DROP COLUMN [PricingMode];
                """);
        }
    }
}
