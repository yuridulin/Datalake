using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddUserReferenceToLogs : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<Guid>(
					name: "UserGuid",
					schema: "public",
					table: "Logs",
					type: "uuid",
					nullable: true);

			migrationBuilder.CreateIndex(
					name: "IX_Logs_UserGuid",
					schema: "public",
					table: "Logs",
					column: "UserGuid");

			migrationBuilder.AddForeignKey(
					name: "FK_Logs_Users_UserGuid",
					schema: "public",
					table: "Logs",
					column: "UserGuid",
					principalSchema: "public",
					principalTable: "Users",
					principalColumn: "Guid");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_Logs_Users_UserGuid",
					schema: "public",
					table: "Logs");

			migrationBuilder.DropIndex(
					name: "IX_Logs_UserGuid",
					schema: "public",
					table: "Logs");

			migrationBuilder.DropColumn(
					name: "UserGuid",
					schema: "public",
					table: "Logs");
		}
	}
}
