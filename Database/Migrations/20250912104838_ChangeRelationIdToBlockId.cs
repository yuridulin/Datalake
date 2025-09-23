using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class ChangeRelationIdToBlockId : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
					UPDATE ""TagInputs"" ti 
					SET
						""InputTagRelationId"" = ""BlockId""
					FROM ""TagInputs"" src
					LEFT OUTER JOIN ""BlockTags"" bt ON bt.""Id"" = src.""InputTagRelationId"" 
					WHERE ti.""Id"" = src.""Id""");

			migrationBuilder.Sql(@"
					UPDATE ""Tags"" t 
					SET
						""SourceTagRelationId"" = bt1.""BlockId"",
						""ThresholdSourceTagRelationId"" = bt2.""BlockId""
					FROM ""Tags"" src 
					LEFT OUTER JOIN ""BlockTags"" bt1 ON bt1.""Id"" = src.""SourceTagRelationId"" 
					LEFT OUTER JOIN ""BlockTags"" bt2 ON bt2.""Id"" = src.""ThresholdSourceTagRelationId"" 
					WHERE t.""Id"" = src.""Id""");

			migrationBuilder.RenameColumn(
					name: "ThresholdSourceTagRelationId",
					schema: "public",
					table: "Tags",
					newName: "ThresholdSourceTagBlockId");

			migrationBuilder.RenameColumn(
					name: "SourceTagRelationId",
					schema: "public",
					table: "Tags",
					newName: "SourceTagBlockId");

			migrationBuilder.RenameColumn(
					name: "InputTagRelationId",
					schema: "public",
					table: "TagInputs",
					newName: "InputBlockId");

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

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
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

			migrationBuilder.RenameColumn(
					name: "ThresholdSourceTagBlockId",
					schema: "public",
					table: "Tags",
					newName: "ThresholdSourceTagRelationId");

			migrationBuilder.RenameColumn(
					name: "SourceTagBlockId",
					schema: "public",
					table: "Tags",
					newName: "SourceTagRelationId");

			migrationBuilder.RenameColumn(
					name: "InputBlockId",
					schema: "public",
					table: "TagInputs",
					newName: "InputTagRelationId");
		}
	}
}
