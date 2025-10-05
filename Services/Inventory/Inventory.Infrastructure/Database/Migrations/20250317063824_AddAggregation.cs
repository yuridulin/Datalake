using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddAggregation : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(
				name: "Aggregation",
				schema: "public",
				table: "Tags",
				type: "integer",
				nullable: true);

		migrationBuilder.AddColumn<int>(
				name: "AggregationPeriod",
				schema: "public",
				table: "Tags",
				type: "integer",
				nullable: true);

		migrationBuilder.AddColumn<int>(
				name: "SourceTagId",
				schema: "public",
				table: "Tags",
				type: "integer",
				nullable: true);

		migrationBuilder.CreateIndex(
				name: "IX_Tags_SourceTagId",
				schema: "public",
				table: "Tags",
				column: "SourceTagId");

		migrationBuilder.AddForeignKey(
				name: "FK_Tags_Tags_SourceTagId",
				schema: "public",
				table: "Tags",
				column: "SourceTagId",
				principalSchema: "public",
				principalTable: "Tags",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetNull);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
				name: "FK_Tags_Tags_SourceTagId",
				schema: "public",
				table: "Tags");

		migrationBuilder.DropIndex(
				name: "IX_Tags_SourceTagId",
				schema: "public",
				table: "Tags");

		migrationBuilder.DropColumn(
				name: "Aggregation",
				schema: "public",
				table: "Tags");

		migrationBuilder.DropColumn(
				name: "AggregationPeriod",
				schema: "public",
				table: "Tags");

		migrationBuilder.DropColumn(
				name: "SourceTagId",
				schema: "public",
				table: "Tags");
	}
}
