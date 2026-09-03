using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260903020000_AddEmailCampaigns")]
    public partial class AddEmailCampaigns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "OptOutEmailCampaigns",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailInvalid",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "EmailCampaigns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BatchSize = table.Column<int>(type: "int", nullable: false, defaultValue: 15),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecipientCount = table.Column<int>(type: "int", nullable: false),
                    SentCount = table.Column<int>(type: "int", nullable: false),
                    FailedCount = table.Column<int>(type: "int", nullable: false),
                    SkippedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailCampaigns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmailCampaignRecipients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmailCampaignId = table.Column<int>(type: "int", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Error = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailCampaignRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailCampaignRecipients_EmailCampaigns_EmailCampaignId",
                        column: x => x.EmailCampaignId,
                        principalTable: "EmailCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmailCampaignRecipients_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmailCampaigns_Status",
                table: "EmailCampaigns",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmailCampaigns_CreatedAt",
                table: "EmailCampaigns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmailCampaignRecipients_Campaign_Patient",
                table: "EmailCampaignRecipients",
                columns: new[] { "EmailCampaignId", "PatientId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmailCampaignRecipients_Status_Campaign",
                table: "EmailCampaignRecipients",
                columns: new[] { "Status", "EmailCampaignId" });

            migrationBuilder.CreateIndex(
                name: "IX_EmailCampaignRecipients_PatientId",
                table: "EmailCampaignRecipients",
                column: "PatientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "EmailCampaignRecipients");
            migrationBuilder.DropTable(name: "EmailCampaigns");
            migrationBuilder.DropColumn(name: "OptOutEmailCampaigns", table: "Patients");
            migrationBuilder.DropColumn(name: "EmailInvalid", table: "Patients");
        }
    }
}
