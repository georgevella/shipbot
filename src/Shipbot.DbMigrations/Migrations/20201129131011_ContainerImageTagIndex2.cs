using Microsoft.EntityFrameworkCore.Migrations;

namespace Shipbot.DbMigrations.Migrations
{
    public partial class ContainerImageTagIndex2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_containerImageRepositories_Name",
                table: "containerImageRepositories");

            migrationBuilder.CreateIndex(
                name: "IX_containerImageRepositories_Name",
                table: "containerImageRepositories",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_containerImageRepositories_Name",
                table: "containerImageRepositories");

            migrationBuilder.CreateIndex(
                name: "IX_containerImageRepositories_Name",
                table: "containerImageRepositories",
                column: "Name");
        }
    }
}
