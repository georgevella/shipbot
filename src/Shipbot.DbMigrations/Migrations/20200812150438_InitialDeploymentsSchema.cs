using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Shipbot.DbMigrations.Migrations
{
    public partial class InitialDeploymentsSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deployments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreationDateTime = table.Column<DateTime>(nullable: false),
                    DeploymentDateTime = table.Column<DateTime>(nullable: false),
                    ApplicationId = table.Column<string>(nullable: false),
                    ImageRepository = table.Column<string>(nullable: false),
                    UpdatePath = table.Column<string>(nullable: false),
                    CurrentImageTag = table.Column<string>(nullable: false),
                    NewImageTag = table.Column<string>(nullable: false),
                    IsAutomaticDeployment = table.Column<bool>(nullable: false),
                    Status = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deployments", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deployments");
        }
    }
}
