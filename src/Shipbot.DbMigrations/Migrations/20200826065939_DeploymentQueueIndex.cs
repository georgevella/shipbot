using Microsoft.EntityFrameworkCore.Migrations;

namespace Shipbot.DbMigrations.Migrations
{
    public partial class DeploymentQueueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_deploymentQueue_AvailableDateTime_AcknowledgeDateTime",
                table: "deploymentQueue",
                columns: new[] { "AvailableDateTime", "AcknowledgeDateTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_deploymentQueue_AvailableDateTime_AcknowledgeDateTime",
                table: "deploymentQueue");
        }
    }
}
