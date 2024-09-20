using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddParentChildrenRelationsToBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Blocks_ParentId",
                schema: "public",
                table: "Blocks",
                column: "ParentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Blocks_Blocks_ParentId",
                schema: "public",
                table: "Blocks",
                column: "ParentId",
                principalSchema: "public",
                principalTable: "Blocks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Blocks_Blocks_ParentId",
                schema: "public",
                table: "Blocks");

            migrationBuilder.DropIndex(
                name: "IX_Blocks_ParentId",
                schema: "public",
                table: "Blocks");
        }
    }
}
