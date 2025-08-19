using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddEnergoIdView : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
					name: "IX_Users_EnergoIdGuid",
					schema: "public",
					table: "Users",
					column: "EnergoIdGuid",
					unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
					name: "IX_Users_EnergoIdGuid",
					schema: "public",
					table: "Users");
		}
	}
}
