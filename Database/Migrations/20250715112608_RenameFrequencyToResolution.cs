using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations;

/// <inheritdoc />
public partial class RenameFrequencyToResolution : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.RenameColumn(
				name: "Frequency",
				schema: "public",
				table: "Tags",
				newName: "Resolution");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.RenameColumn(
				name: "Resolution",
				schema: "public",
				table: "Tags",
				newName: "Frequency");
	}
}
