using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Shipbot.DbMigrations.Migrations
{
    public partial class DeploymentQueue3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "UpdatedDateTime",
                table: "slackMessages",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreationDateTime",
                table: "slackMessages",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateIndex(
                name: "IX_deploymentNotifications_DeploymentId",
                table: "deploymentNotifications",
                column: "DeploymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_deploymentNotifications_deployments_DeploymentId",
                table: "deploymentNotifications",
                column: "DeploymentId",
                principalTable: "deployments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deploymentNotifications_deployments_DeploymentId",
                table: "deploymentNotifications");

            migrationBuilder.DropIndex(
                name: "IX_deploymentNotifications_DeploymentId",
                table: "deploymentNotifications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDateTime",
                table: "slackMessages",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset));

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreationDateTime",
                table: "slackMessages",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTimeOffset));
        }
    }
}
