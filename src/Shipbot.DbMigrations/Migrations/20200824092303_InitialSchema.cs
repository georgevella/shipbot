using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Shipbot.DbMigrations.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deployments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationDateTime = table.Column<DateTime>(nullable: false),
                    DeploymentDateTime = table.Column<DateTime>(nullable: true),
                    ApplicationId = table.Column<string>(nullable: false),
                    ImageRepository = table.Column<string>(nullable: false),
                    UpdatePath = table.Column<string>(nullable: false),
                    CurrentImageTag = table.Column<string>(nullable: false),
                    TargetImageTag = table.Column<string>(nullable: false),
                    IsAutomaticDeployment = table.Column<bool>(nullable: false),
                    Status = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deployments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "slackMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Timestamp = table.Column<string>(nullable: false),
                    ChannelId = table.Column<string>(nullable: false),
                    CreationDateTime = table.Column<DateTime>(nullable: false),
                    UpdatedDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_slackMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "deploymentNotifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SlackMessageId = table.Column<Guid>(nullable: false),
                    DeploymentId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deploymentNotifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_deploymentNotifications_slackMessages_SlackMessageId",
                        column: x => x.SlackMessageId,
                        principalTable: "slackMessages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deploymentNotifications_SlackMessageId",
                table: "deploymentNotifications",
                column: "SlackMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_ApplicationId_ImageRepository_UpdatePath_Curren~",
                table: "deployments",
                columns: new[] { "ApplicationId", "ImageRepository", "UpdatePath", "CurrentImageTag", "TargetImageTag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_slackMessages_Timestamp_ChannelId",
                table: "slackMessages",
                columns: new[] { "Timestamp", "ChannelId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deploymentNotifications");

            migrationBuilder.DropTable(
                name: "deployments");

            migrationBuilder.DropTable(
                name: "slackMessages");
        }
    }
}
