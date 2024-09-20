using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddEnergoIdApiToSettings : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
					name: "public");

			migrationBuilder.RenameTable(
					name: "Users",
					newName: "Users",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "UserGroups",
					newName: "UserGroups",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "UserGroupRelation",
					newName: "UserGroupRelation",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "TagsLive",
					newName: "TagsLive",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "Tags",
					newName: "Tags",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "TagInputs",
					newName: "TagInputs",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "TagHistoryChunks",
					newName: "TagHistoryChunks",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "Sources",
					newName: "Sources",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "Settings",
					newName: "Settings",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "Logs",
					newName: "Logs",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "BlockTags",
					newName: "BlockTags",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "Blocks",
					newName: "Blocks",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "BlockProperties",
					newName: "BlockProperties",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "AccessRights",
					newName: "AccessRights",
					newSchema: "public");

			migrationBuilder.RenameColumn(
					name: "EnergoIdHost",
					schema: "public",
					table: "Settings",
					newName: "KeycloakHost");

			migrationBuilder.AddColumn<string>(
					name: "EnergoIdApi",
					schema: "public",
					table: "Settings",
					type: "text",
					nullable: false,
					defaultValue: "");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
					name: "EnergoIdApi",
					schema: "public",
					table: "Settings");

			migrationBuilder.RenameTable(
					name: "Users",
					schema: "public",
					newName: "Users");

			migrationBuilder.RenameTable(
					name: "UserGroups",
					schema: "public",
					newName: "UserGroups");

			migrationBuilder.RenameTable(
					name: "UserGroupRelation",
					schema: "public",
					newName: "UserGroupRelation");

			migrationBuilder.RenameTable(
					name: "TagsLive",
					schema: "public",
					newName: "TagsLive");

			migrationBuilder.RenameTable(
					name: "Tags",
					schema: "public",
					newName: "Tags");

			migrationBuilder.RenameTable(
					name: "TagInputs",
					schema: "public",
					newName: "TagInputs");

			migrationBuilder.RenameTable(
					name: "TagHistoryChunks",
					schema: "public",
					newName: "TagHistoryChunks");

			migrationBuilder.RenameTable(
					name: "Sources",
					schema: "public",
					newName: "Sources");

			migrationBuilder.RenameTable(
					name: "Settings",
					schema: "public",
					newName: "Settings");

			migrationBuilder.RenameTable(
					name: "Logs",
					schema: "public",
					newName: "Logs");

			migrationBuilder.RenameTable(
					name: "BlockTags",
					schema: "public",
					newName: "BlockTags");

			migrationBuilder.RenameTable(
					name: "Blocks",
					schema: "public",
					newName: "Blocks");

			migrationBuilder.RenameTable(
					name: "BlockProperties",
					schema: "public",
					newName: "BlockProperties");

			migrationBuilder.RenameTable(
					name: "AccessRights",
					schema: "public",
					newName: "AccessRights");

			migrationBuilder.RenameColumn(
					name: "KeycloakHost",
					table: "Settings",
					newName: "EnergoIdHost");
		}
	}
}
