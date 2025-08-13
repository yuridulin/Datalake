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

			migrationBuilder.AddColumn<string>(
					name: "Thresholds",
					schema: "public",
					table: "Tags",
					type: "jsonb",
					nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
					name: "Calculation",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropColumn(
					name: "Thresholds",
					schema: "public",
					table: "Tags");
		}
	}
}
