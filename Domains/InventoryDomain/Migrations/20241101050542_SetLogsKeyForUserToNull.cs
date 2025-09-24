using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations;

/// <inheritdoc />
public partial class SetLogsKeyForUserToNull : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
				name: "FK_Logs_Users_UserGuid",
				schema: "public",
				table: "Logs");

		migrationBuilder.AddColumn<int>(
				name: "Id",
				schema: "public",
				table: "UserGroupRelation",
				type: "integer",
				nullable: false,
				defaultValue: 0);

		migrationBuilder.AddForeignKey(
				name: "FK_Logs_Users_UserGuid",
				schema: "public",
				table: "Logs",
				column: "UserGuid",
				principalSchema: "public",
				principalTable: "Users",
				principalColumn: "Guid",
				onDelete: ReferentialAction.SetNull);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
				name: "FK_Logs_Users_UserGuid",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropColumn(
				name: "Id",
				schema: "public",
				table: "UserGroupRelation");

		migrationBuilder.AddForeignKey(
				name: "FK_Logs_Users_UserGuid",
				schema: "public",
				table: "Logs",
				column: "UserGuid",
				principalSchema: "public",
				principalTable: "Users",
				principalColumn: "Guid");
	}
}
