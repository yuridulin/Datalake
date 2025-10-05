using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddRelationIdWhenSelectingTag : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(
				name: "SourceTagRelationId",
				schema: "public",
				table: "Tags",
				type: "integer",
				nullable: true);

		migrationBuilder.AddColumn<int>(
				name: "InputTagRelationId",
				schema: "public",
				table: "TagInputs",
				type: "integer",
				nullable: true);

		migrationBuilder.AddColumn<int>(
				name: "Id",
				schema: "public",
				table: "BlockTags",
				type: "integer",
				nullable: false,
				defaultValue: 0)
				.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropColumn(
				name: "SourceTagRelationId",
				schema: "public",
				table: "Tags");

		migrationBuilder.DropColumn(
				name: "InputTagRelationId",
				schema: "public",
				table: "TagInputs");

		migrationBuilder.DropColumn(
				name: "Id",
				schema: "public",
				table: "BlockTags");
	}
}
