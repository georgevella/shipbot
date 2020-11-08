using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Shipbot.DbMigrations.Migrations
{
    public partial class DeploymentSources : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "containerImageRepositories",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_containerImageRepositories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "containerImageMetadata",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RepositoryId = table.Column<Guid>(nullable: false),
                    Hash = table.Column<string>(nullable: false),
                    CreatedDateTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_containerImageMetadata", x => x.Id);
                    table.ForeignKey(
                        name: "FK_containerImageMetadata_containerImageRepositories_Repositor~",
                        column: x => x.RepositoryId,
                        principalTable: "containerImageRepositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "containerImageTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    RepositoryId = table.Column<Guid>(nullable: false),
                    MetadataId = table.Column<Guid>(nullable: false),
                    Tag = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_containerImageTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_containerImageTags_containerImageMetadata_MetadataId",
                        column: x => x.MetadataId,
                        principalTable: "containerImageMetadata",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_containerImageTags_containerImageRepositories_RepositoryId",
                        column: x => x.RepositoryId,
                        principalTable: "containerImageRepositories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_containerImageMetadata_RepositoryId",
                table: "containerImageMetadata",
                column: "RepositoryId");

            migrationBuilder.CreateIndex(
                name: "IX_containerImageMetadata_RepositoryId_Hash",
                table: "containerImageMetadata",
                columns: new[] { "RepositoryId", "Hash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_containerImageRepositories_Name",
                table: "containerImageRepositories",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_containerImageTags_MetadataId",
                table: "containerImageTags",
                column: "MetadataId");

            migrationBuilder.CreateIndex(
                name: "IX_containerImageTags_RepositoryId_Tag",
                table: "containerImageTags",
                columns: new[] { "RepositoryId", "Tag" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "containerImageTags");

            migrationBuilder.DropTable(
                name: "containerImageMetadata");

            migrationBuilder.DropTable(
                name: "containerImageRepositories");
        }
    }
}
