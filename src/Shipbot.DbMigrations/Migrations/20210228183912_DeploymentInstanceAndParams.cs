using Microsoft.EntityFrameworkCore.Migrations;

namespace Shipbot.DbMigrations.Migrations
{
    public partial class DeploymentInstanceAndParams : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstanceId",
                table: "deployments",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Parameters",
                table: "deployments",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstanceId",
                table: "deployments");

            migrationBuilder.DropColumn(
                name: "Parameters",
                table: "deployments");
        }
    }
}
