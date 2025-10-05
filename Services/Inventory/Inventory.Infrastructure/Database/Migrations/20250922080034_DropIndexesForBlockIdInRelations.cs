using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class DropIndexesForBlockIdInRelations : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_TagInputs_Tags_InputBlockId",
					schema: "public",
					table: "TagInputs");

			migrationBuilder.DropForeignKey(
					name: "FK_Tags_Blocks_SourceTagBlockId",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropForeignKey(
					name: "FK_Tags_Blocks_ThresholdSourceTagBlockId",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropIndex(
					name: "IX_Tags_SourceTagBlockId",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropIndex(
					name: "IX_Tags_ThresholdSourceTagBlockId",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropIndex(
					name: "IX_TagInputs_InputBlockId",
					schema: "public",
					table: "TagInputs");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
					name: "IX_Tags_SourceTagBlockId",
					schema: "public",
					table: "Tags",
					column: "SourceTagBlockId");

			migrationBuilder.CreateIndex(
					name: "IX_Tags_ThresholdSourceTagBlockId",
					schema: "public",
					table: "Tags",
					column: "ThresholdSourceTagBlockId");

			migrationBuilder.CreateIndex(
					name: "IX_TagInputs_InputBlockId",
					schema: "public",
					table: "TagInputs",
					column: "InputBlockId");

			migrationBuilder.AddForeignKey(
					name: "FK_TagInputs_Tags_InputBlockId",
					schema: "public",
					table: "TagInputs",
					column: "InputBlockId",
					principalSchema: "public",
					principalTable: "Tags",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);

			migrationBuilder.AddForeignKey(
					name: "FK_Tags_Blocks_SourceTagBlockId",
					schema: "public",
					table: "Tags",
					column: "SourceTagBlockId",
					principalSchema: "public",
					principalTable: "Blocks",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);

			migrationBuilder.AddForeignKey(
					name: "FK_Tags_Blocks_ThresholdSourceTagBlockId",
					schema: "public",
					table: "Tags",
					column: "ThresholdSourceTagBlockId",
					principalSchema: "public",
					principalTable: "Blocks",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);
		}
	}
}
