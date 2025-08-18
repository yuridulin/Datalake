using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations;

/// <inheritdoc />
public partial class SwitchToTimescale : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
				name: "TagsHistory",
				schema: "public",
				columns: table => new
				{
					TagId = table.Column<int>(type: "integer", nullable: false),
					Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
					Text = table.Column<string>(type: "text", nullable: true),
					Number = table.Column<float>(type: "real", nullable: true),
					Quality = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
				});
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
				name: "TagsHistory",
				schema: "public");
	}
}
