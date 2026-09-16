using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260904520000_AddDoctorCanViewAllClinicData")]
    public partial class AddDoctorCanViewAllClinicData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Doctors', N'CanViewAllClinicData') IS NULL
                BEGIN
                    ALTER TABLE [Doctors]
                    ADD [CanViewAllClinicData] bit NOT NULL
                    CONSTRAINT [DF_Doctors_CanViewAllClinicData] DEFAULT (1);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.Doctors', N'CanViewAllClinicData') IS NOT NULL
                BEGIN
                    ALTER TABLE [Doctors] DROP CONSTRAINT [DF_Doctors_CanViewAllClinicData];
                    ALTER TABLE [Doctors] DROP COLUMN [CanViewAllClinicData];
                END
                """);
        }
    }
}
