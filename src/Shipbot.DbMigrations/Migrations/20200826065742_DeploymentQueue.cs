using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Shipbot.DbMigrations.Migrations
{
    public partial class DeploymentQueue : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deploymentQueue",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DeploymentId = table.Column<Guid>(nullable: false),
                    ApplicationId = table.Column<string>(nullable: false),
                    CreationDateTime = table.Column<DateTime>(nullable: false),
                    AvailableDateTime = table.Column<DateTime>(nullable: false),
                    AcknowledgeDateTime = table.Column<DateTime>(nullable: true),
                    AttemptCount = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deploymentQueue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_deploymentQueue_deployments_DeploymentId",
                        column: x => x.DeploymentId,
                        principalTable: "deployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deploymentQueue_DeploymentId",
                table: "deploymentQueue",
                column: "DeploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_deploymentQueue_ApplicationId_AvailableDateTime_Acknowledge~",
                table: "deploymentQueue",
                columns: new[] { "ApplicationId", "AvailableDateTime", "AcknowledgeDateTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deploymentQueue");
        }
    }
}
