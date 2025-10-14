using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class ChangeSchema : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_AccessRights_Blocks_BlockId",
					schema: "public",
					table: "AccessRights");

			migrationBuilder.DropForeignKey(
					name: "FK_AccessRights_Sources_SourceId",
					schema: "public",
					table: "AccessRights");

			migrationBuilder.DropForeignKey(
					name: "FK_AccessRights_Tags_TagId",
					schema: "public",
					table: "AccessRights");

			migrationBuilder.DropForeignKey(
					name: "FK_AccessRights_UserGroups_UserGroupGuid",
					schema: "public",
					table: "AccessRights");

			migrationBuilder.DropForeignKey(
					name: "FK_AccessRights_Users_UserGuid",
					schema: "public",
					table: "AccessRights");

			migrationBuilder.DropForeignKey(
					name: "FK_CalculatedAccessRules_AccessRights_RuleId",
					schema: "public",
					table: "CalculatedAccessRules");

			migrationBuilder.DropForeignKey(
					name: "FK_Logs_AccessRights_AffectedAccessRightsId",
					schema: "public",
					table: "Logs");

			migrationBuilder.DropIndex(
					name: "IX_CalculatedAccessRules_RuleId",
					schema: "public",
					table: "CalculatedAccessRules");

			migrationBuilder.DropPrimaryKey(
					name: "PK_AccessRights",
					schema: "public",
					table: "AccessRights");

			migrationBuilder.EnsureSchema(
					name: "inventory");

			migrationBuilder.RenameTable(
					name: "Users",
					schema: "public",
					newName: "Users",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "UserGroups",
					schema: "public",
					newName: "UserGroups",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "UserGroupRelations",
					schema: "public",
					newName: "UserGroupRelations",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "TagThresholds",
					schema: "public",
					newName: "TagThresholds",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "Tags",
					schema: "public",
					newName: "Tags",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "TagInputs",
					schema: "public",
					newName: "TagInputs",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "Sources",
					schema: "public",
					newName: "Sources",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "Settings",
					schema: "public",
					newName: "Settings",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "Logs",
					schema: "public",
					newName: "Logs",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "CalculatedAccessRules",
					schema: "public",
					newName: "CalculatedAccessRules",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "BlockTags",
					schema: "public",
					newName: "BlockTags",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "Blocks",
					schema: "public",
					newName: "Blocks",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "BlockProperties",
					schema: "public",
					newName: "BlockProperties",
					newSchema: "inventory");

			migrationBuilder.RenameTable(
					name: "AccessRights",
					schema: "public",
					newName: "AccessRules",
					newSchema: "inventory");

			migrationBuilder.RenameIndex(
					name: "IX_AccessRights_UserGuid",
					schema: "inventory",
					table: "AccessRules",
					newName: "IX_AccessRules_UserGuid");

			migrationBuilder.RenameIndex(
					name: "IX_AccessRights_UserGroupGuid",
					schema: "inventory",
					table: "AccessRules",
					newName: "IX_AccessRules_UserGroupGuid");

			migrationBuilder.RenameIndex(
					name: "IX_AccessRights_TagId",
					schema: "inventory",
					table: "AccessRules",
					newName: "IX_AccessRules_TagId");

			migrationBuilder.RenameIndex(
					name: "IX_AccessRights_SourceId",
					schema: "inventory",
					table: "AccessRules",
					newName: "IX_AccessRules_SourceId");

			migrationBuilder.RenameIndex(
					name: "IX_AccessRights_BlockId",
					schema: "inventory",
					table: "AccessRules",
					newName: "IX_AccessRules_BlockId");

			migrationBuilder.AddPrimaryKey(
					name: "PK_AccessRules",
					schema: "inventory",
					table: "AccessRules",
					column: "Id");

			migrationBuilder.AddForeignKey(
					name: "FK_AccessRules_Blocks_BlockId",
					schema: "inventory",
					table: "AccessRules",
					column: "BlockId",
					principalSchema: "inventory",
					principalTable: "Blocks",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);

			migrationBuilder.AddForeignKey(
					name: "FK_AccessRules_Sources_SourceId",
					schema: "inventory",
					table: "AccessRules",
					column: "SourceId",
					principalSchema: "inventory",
					principalTable: "Sources",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);

			migrationBuilder.AddForeignKey(
					name: "FK_AccessRules_Tags_TagId",
					schema: "inventory",
					table: "AccessRules",
					column: "TagId",
					principalSchema: "inventory",
					principalTable: "Tags",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);

			migrationBuilder.AddForeignKey(
					name: "FK_AccessRules_UserGroups_UserGroupGuid",
					schema: "inventory",
					table: "AccessRules",
					column: "UserGroupGuid",
					principalSchema: "inventory",
					principalTable: "UserGroups",
					principalColumn: "Guid",
					onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
					name: "FK_AccessRules_Users_UserGuid",
					schema: "inventory",
					table: "AccessRules",
					column: "UserGuid",
					principalSchema: "inventory",
					principalTable: "Users",
					principalColumn: "Guid",
					onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
					name: "FK_Logs_AccessRules_AffectedAccessRightsId",
					schema: "inventory",
					table: "Logs",
					column: "AffectedAccessRightsId",
					principalSchema: "inventory",
					principalTable: "AccessRules",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_AccessRules_Blocks_BlockId",
					schema: "inventory",
					table: "AccessRules");

			migrationBuilder.DropForeignKey(
					name: "FK_AccessRules_Sources_SourceId",
					schema: "inventory",
					table: "AccessRules");

			migrationBuilder.DropForeignKey(
					name: "FK_AccessRules_Tags_TagId",
					schema: "inventory",
					table: "AccessRules");

			migrationBuilder.DropForeignKey(
					name: "FK_AccessRules_UserGroups_UserGroupGuid",
					schema: "inventory",
					table: "AccessRules");

			migrationBuilder.DropForeignKey(
					name: "FK_AccessRules_Users_UserGuid",
					schema: "inventory",
					table: "AccessRules");

			migrationBuilder.DropForeignKey(
					name: "FK_Logs_AccessRules_AffectedAccessRightsId",
					schema: "inventory",
					table: "Logs");

			migrationBuilder.DropPrimaryKey(
					name: "PK_AccessRules",
					schema: "inventory",
					table: "AccessRules");

			migrationBuilder.EnsureSchema(
					name: "public");

			migrationBuilder.RenameTable(
					name: "Users",
					schema: "inventory",
					newName: "Users",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "UserGroups",
					schema: "inventory",
					newName: "UserGroups",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "UserGroupRelations",
					schema: "inventory",
					newName: "UserGroupRelations",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "TagThresholds",
					schema: "inventory",
					newName: "TagThresholds",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "Tags",
					schema: "inventory",
					newName: "Tags",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "TagInputs",
					schema: "inventory",
					newName: "TagInputs",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "Sources",
					schema: "inventory",
					newName: "Sources",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "Settings",
					schema: "inventory",
					newName: "Settings",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "Logs",
					schema: "inventory",
					newName: "Logs",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "CalculatedAccessRules",
					schema: "inventory",
					newName: "CalculatedAccessRules",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "BlockTags",
					schema: "inventory",
					newName: "BlockTags",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "Blocks",
					schema: "inventory",
					newName: "Blocks",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "BlockProperties",
					schema: "inventory",
					newName: "BlockProperties",
					newSchema: "public");

			migrationBuilder.RenameTable(
					name: "AccessRules",
					schema: "inventory",
					newName: "AccessRights",
					newSchema: "public");

			migrationBuilder.RenameIndex(
					name: "IX_AccessRules_UserGuid",
					schema: "public",
					table: "AccessRights",
					newName: "IX_AccessRights_UserGuid");

			migrationBuilder.RenameIndex(
					name: "IX_AccessRules_UserGroupGuid",
					schema: "public",
					table: "AccessRights",
					newName: "IX_AccessRights_UserGroupGuid");

			migrationBuilder.RenameIndex(
					name: "IX_AccessRules_TagId",
					schema: "public",
					table: "AccessRights",
					newName: "IX_AccessRights_TagId");

			migrationBuilder.RenameIndex(
					name: "IX_AccessRules_SourceId",
					schema: "public",
					table: "AccessRights",
					newName: "IX_AccessRights_SourceId");

			migrationBuilder.RenameIndex(
					name: "IX_AccessRules_BlockId",
					schema: "public",
					table: "AccessRights",
					newName: "IX_AccessRights_BlockId");

			migrationBuilder.AddPrimaryKey(
					name: "PK_AccessRights",
					schema: "public",
					table: "AccessRights",
					column: "Id");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_RuleId",
					schema: "public",
					table: "CalculatedAccessRules",
					column: "RuleId");

			migrationBuilder.AddForeignKey(
					name: "FK_AccessRights_Blocks_BlockId",
					schema: "public",
					table: "AccessRights",
					column: "BlockId",
					principalSchema: "public",
					principalTable: "Blocks",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);

			migrationBuilder.AddForeignKey(
					name: "FK_AccessRights_Sources_SourceId",
					schema: "public",
					table: "AccessRights",
					column: "SourceId",
					principalSchema: "public",
					principalTable: "Sources",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);

			migrationBuilder.AddForeignKey(
					name: "FK_AccessRights_Tags_TagId",
					schema: "public",
					table: "AccessRights",
					column: "TagId",
					principalSchema: "public",
					principalTable: "Tags",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);

			migrationBuilder.AddForeignKey(
					name: "FK_AccessRights_UserGroups_UserGroupGuid",
					schema: "public",
					table: "AccessRights",
					column: "UserGroupGuid",
					principalSchema: "public",
					principalTable: "UserGroups",
					principalColumn: "Guid",
					onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
					name: "FK_AccessRights_Users_UserGuid",
					schema: "public",
					table: "AccessRights",
					column: "UserGuid",
					principalSchema: "public",
					principalTable: "Users",
					principalColumn: "Guid",
					onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
					name: "FK_CalculatedAccessRules_AccessRights_RuleId",
					schema: "public",
					table: "CalculatedAccessRules",
					column: "RuleId",
					principalSchema: "public",
					principalTable: "AccessRights",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
					name: "FK_Logs_AccessRights_AffectedAccessRightsId",
					schema: "public",
					table: "Logs",
					column: "AffectedAccessRightsId",
					principalSchema: "public",
					principalTable: "AccessRights",
					principalColumn: "Id",
					onDelete: ReferentialAction.SetNull);
		}
	}
}
