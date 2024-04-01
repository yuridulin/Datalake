using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatalakeDatabase.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Sources_SourceId",
                table: "Tags");

            migrationBuilder.AlterColumn<int>(
                name: "SourceId",
                table: "Tags",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ParentId",
                table: "Blocks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_TagInputs_TagId",
                table: "TagInputs",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlockTags_Blocks_BlockId",
                table: "BlockTags",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BlockTags_Tags_TagId",
                table: "BlockTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TagInputs_Tags_TagId",
                table: "TagInputs",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Sources_SourceId",
                table: "Tags",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlockTags_Blocks_BlockId",
                table: "BlockTags");

            migrationBuilder.DropForeignKey(
                name: "FK_BlockTags_Tags_TagId",
                table: "BlockTags");

            migrationBuilder.DropForeignKey(
                name: "FK_TagInputs_Tags_TagId",
                table: "TagInputs");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Sources_SourceId",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_TagInputs_TagId",
                table: "TagInputs");

            migrationBuilder.AlterColumn<int>(
                name: "SourceId",
                table: "Tags",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ParentId",
                table: "Blocks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Sources_SourceId",
                table: "Tags",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
