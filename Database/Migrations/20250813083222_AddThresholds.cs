using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddThresholds : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
					name: "Calculation",
					schema: "public",
					table: "Tags",
					type: "integer",
					nullable: true);

			migrationBuilder.AddColumn<int>(
					name: "ThresholdSourceTagId",
					schema: "public",
					table: "Tags",
					type: "integer",
					nullable: true);

			migrationBuilder.AddColumn<int>(
					name: "ThresholdSourceTagRelationId",
					schema: "public",
					table: "Tags",
					type: "integer",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "Thresholds",
					schema: "public",
					table: "Tags",
					type: "jsonb",
					nullable: true);

			migrationBuilder.CreateIndex(
					name: "IX_Tags_ThresholdSourceTagId",
					schema: "public",
					table: "Tags",
					column: "ThresholdSourceTagId");

			migrationBuilder.AddForeignKey(
					name: "FK_Tags_Tags_ThresholdSourceTagId",
					schema: "public",
					table: "Tags",
					column: "ThresholdSourceTagId",
					principalSchema: "public",
					principalTable: "Tags",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_Tags_Tags_ThresholdSourceTagId",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropIndex(
					name: "IX_Tags_ThresholdSourceTagId",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropColumn(
					name: "Calculation",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropColumn(
					name: "ThresholdSourceTagId",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropColumn(
					name: "ThresholdSourceTagRelationId",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropColumn(
					name: "Thresholds",
					schema: "public",
					table: "Tags");
		}
	}
}
