using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDoctorSignatureImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignatureImageUrl",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureImageUrl",
                table: "Doctors");
        }
    }
}
