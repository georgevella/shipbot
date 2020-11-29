using Microsoft.EntityFrameworkCore.Migrations;

namespace Shipbot.DbMigrations.Migrations
{
    public partial class ContainerImageTagIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_containerImageTags_RepositoryId_Tag",
                table: "containerImageTags");

            migrationBuilder.CreateIndex(
                name: "IX_containerImageTags_RepositoryId_MetadataId",
                table: "containerImageTags",
                columns: new[] { "RepositoryId", "MetadataId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_containerImageTags_RepositoryId_Tag",
                table: "containerImageTags",
                columns: new[] { "RepositoryId", "Tag" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_containerImageTags_RepositoryId_MetadataId",
                table: "containerImageTags");

            migrationBuilder.DropIndex(
                name: "IX_containerImageTags_RepositoryId_Tag",
                table: "containerImageTags");

            migrationBuilder.CreateIndex(
                name: "IX_containerImageTags_RepositoryId_Tag",
                table: "containerImageTags",
                columns: new[] { "RepositoryId", "Tag" });
        }
    }
}
