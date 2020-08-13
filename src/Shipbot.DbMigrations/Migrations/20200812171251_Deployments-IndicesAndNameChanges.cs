using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Shipbot.DbMigrations.Migrations
{
    public partial class DeploymentsIndicesAndNameChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewImageTag",
                table: "deployments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeploymentDateTime",
                table: "deployments",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<string>(
                name: "TargetImageTag",
                table: "deployments",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_deployments_ApplicationId_ImageRepository_UpdatePath_Curren~",
                table: "deployments",
                columns: new[] { "ApplicationId", "ImageRepository", "UpdatePath", "CurrentImageTag", "TargetImageTag" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_deployments_ApplicationId_ImageRepository_UpdatePath_Curren~",
                table: "deployments");

            migrationBuilder.DropColumn(
                name: "TargetImageTag",
                table: "deployments");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DeploymentDateTime",
                table: "deployments",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewImageTag",
                table: "deployments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
