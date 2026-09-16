using Clinic_System.Data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic_System.Data.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260904510000_AddPasswordRecoveryRequests")]
    public partial class AddPasswordRecoveryRequests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF OBJECT_ID(N'dbo.PasswordRecoveryRequests', N'U') IS NULL
                BEGIN
                    CREATE TABLE [PasswordRecoveryRequests] (
                        [Id] int NOT NULL IDENTITY,
                        [Identifier] nvarchar(256) NOT NULL,
                        [NormalizedIdentifier] nvarchar(256) NOT NULL,
                        [Comment] nvarchar(500) NULL,
                        [UserId] nvarchar(450) NULL,
                        [UserEmail] nvarchar(256) NULL,
                        [UserName] nvarchar(256) NULL,
                        [UserDisplayName] nvarchar(256) NULL,
                        [UserType] nvarchar(40) NULL,
                        [UserMatched] bit NOT NULL,
                        [Status] nvarchar(40) NOT NULL,
                        [RequestedAt] datetime2 NOT NULL,
                        [ResolvedAt] datetime2 NULL,
                        [ResolvedByUserId] nvarchar(450) NULL,
                        [ResolvedByName] nvarchar(256) NULL,
                        [ResolutionNote] nvarchar(500) NULL,
                        [CreatedAt] datetime2 NOT NULL,
                        [UpdatedAt] datetime2 NULL,
                        CONSTRAINT [PK_PasswordRecoveryRequests] PRIMARY KEY ([Id])
                    );

                    CREATE INDEX [IX_PasswordRecoveryRequests_Status]
                        ON [PasswordRecoveryRequests] ([Status]);

                    CREATE INDEX [IX_PasswordRecoveryRequests_Identifier_Status_Requested]
                        ON [PasswordRecoveryRequests] ([NormalizedIdentifier], [Status], [RequestedAt]);
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PasswordRecoveryRequests");
        }
    }
}
