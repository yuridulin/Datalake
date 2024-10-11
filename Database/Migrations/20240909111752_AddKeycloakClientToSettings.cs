using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddKeycloakClientToSettings : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(
					name: "KeycloakClient",
					schema: "public",
					table: "Settings",
					type: "text",
					nullable: false,
					defaultValue: "datalake");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
					name: "KeycloakClient",
					schema: "public",
					table: "Settings");
		}
	}
}
