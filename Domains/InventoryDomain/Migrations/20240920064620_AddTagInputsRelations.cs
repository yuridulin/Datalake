using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Datalake.Database.Migrations;

/// <inheritdoc />
public partial class AddTagInputsRelations : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(
				name: "Id",
				schema: "public",
				table: "TagInputs",
				type: "integer",
				nullable: false,
				defaultValue: 0)
				.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

		migrationBuilder.AddPrimaryKey(
				name: "PK_TagInputs",
				schema: "public",
				table: "TagInputs",
				column: "Id");

		migrationBuilder.CreateIndex(
				name: "IX_TagInputs_InputTagId",
				schema: "public",
				table: "TagInputs",
				column: "InputTagId");

		migrationBuilder.AddForeignKey(
				name: "FK_TagInputs_Tags_InputTagId",
				schema: "public",
				table: "TagInputs",
				column: "InputTagId",
				principalSchema: "public",
				principalTable: "Tags",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
				name: "FK_TagInputs_Tags_InputTagId",
				schema: "public",
				table: "TagInputs");

		migrationBuilder.DropPrimaryKey(
				name: "PK_TagInputs",
				schema: "public",
				table: "TagInputs");

		migrationBuilder.DropIndex(
				name: "IX_TagInputs_InputTagId",
				schema: "public",
				table: "TagInputs");

		migrationBuilder.DropColumn(
				name: "Id",
				schema: "public",
				table: "TagInputs");
	}
}
