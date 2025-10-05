using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Migrations;

/// <inheritdoc />
public partial class FakeDeleteToObjectsAndLogRelations : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
				name: "FK_Logs_Users_UserGuid",
				schema: "public",
				table: "Logs");

		migrationBuilder.RenameColumn(
				name: "UserGuid",
				schema: "public",
				table: "Logs",
				newName: "AuthorGuid");

		migrationBuilder.RenameIndex(
				name: "IX_Logs_UserGuid",
				schema: "public",
				table: "Logs",
				newName: "IX_Logs_AuthorGuid");

		migrationBuilder.AddColumn<bool>(
				name: "IsDeleted",
				schema: "public",
				table: "Users",
				type: "boolean",
				nullable: false,
				defaultValue: false);

		migrationBuilder.AddColumn<bool>(
				name: "IsDeleted",
				schema: "public",
				table: "UserGroups",
				type: "boolean",
				nullable: false,
				defaultValue: false);

		migrationBuilder.AddColumn<bool>(
				name: "IsDeleted",
				schema: "public",
				table: "Tags",
				type: "boolean",
				nullable: false,
				defaultValue: false);

		migrationBuilder.AddColumn<bool>(
				name: "IsDeleted",
				schema: "public",
				table: "Sources",
				type: "boolean",
				nullable: false,
				defaultValue: false);

		migrationBuilder.AddColumn<int>(
				name: "AffectedAccessRightsId",
				schema: "public",
				table: "Logs",
				type: "integer",
				nullable: true);

		migrationBuilder.AddColumn<int>(
				name: "AffectedBlockId",
				schema: "public",
				table: "Logs",
				type: "integer",
				nullable: true);

		migrationBuilder.AddColumn<int>(
				name: "AffectedSourceId",
				schema: "public",
				table: "Logs",
				type: "integer",
				nullable: true);

		migrationBuilder.AddColumn<int>(
				name: "AffectedTagId",
				schema: "public",
				table: "Logs",
				type: "integer",
				nullable: true);

		migrationBuilder.AddColumn<Guid>(
				name: "AffectedUserGroupGuid",
				schema: "public",
				table: "Logs",
				type: "uuid",
				nullable: true);

		migrationBuilder.AddColumn<Guid>(
				name: "AffectedUserGuid",
				schema: "public",
				table: "Logs",
				type: "uuid",
				nullable: true);

		migrationBuilder.AddColumn<bool>(
				name: "IsDeleted",
				schema: "public",
				table: "Blocks",
				type: "boolean",
				nullable: false,
				defaultValue: false);

		migrationBuilder.CreateIndex(
				name: "IX_Logs_AffectedAccessRightsId",
				schema: "public",
				table: "Logs",
				column: "AffectedAccessRightsId");

		migrationBuilder.CreateIndex(
				name: "IX_Logs_AffectedBlockId",
				schema: "public",
				table: "Logs",
				column: "AffectedBlockId");

		migrationBuilder.CreateIndex(
				name: "IX_Logs_AffectedSourceId",
				schema: "public",
				table: "Logs",
				column: "AffectedSourceId");

		migrationBuilder.CreateIndex(
				name: "IX_Logs_AffectedTagId",
				schema: "public",
				table: "Logs",
				column: "AffectedTagId");

		migrationBuilder.CreateIndex(
				name: "IX_Logs_AffectedUserGroupGuid",
				schema: "public",
				table: "Logs",
				column: "AffectedUserGroupGuid");

		migrationBuilder.CreateIndex(
				name: "IX_Logs_AffectedUserGuid",
				schema: "public",
				table: "Logs",
				column: "AffectedUserGuid");

		migrationBuilder.AddForeignKey(
				name: "FK_Logs_AccessRights_AffectedAccessRightsId",
				schema: "public",
				table: "Logs",
				column: "AffectedAccessRightsId",
				principalSchema: "public",
				principalTable: "AccessRights",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetNull);

		migrationBuilder.AddForeignKey(
				name: "FK_Logs_Blocks_AffectedBlockId",
				schema: "public",
				table: "Logs",
				column: "AffectedBlockId",
				principalSchema: "public",
				principalTable: "Blocks",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetNull);

		migrationBuilder.AddForeignKey(
				name: "FK_Logs_Sources_AffectedSourceId",
				schema: "public",
				table: "Logs",
				column: "AffectedSourceId",
				principalSchema: "public",
				principalTable: "Sources",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetNull);

		migrationBuilder.AddForeignKey(
				name: "FK_Logs_Tags_AffectedTagId",
				schema: "public",
				table: "Logs",
				column: "AffectedTagId",
				principalSchema: "public",
				principalTable: "Tags",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetNull);

		migrationBuilder.AddForeignKey(
				name: "FK_Logs_UserGroups_AffectedUserGroupGuid",
				schema: "public",
				table: "Logs",
				column: "AffectedUserGroupGuid",
				principalSchema: "public",
				principalTable: "UserGroups",
				principalColumn: "Guid",
				onDelete: ReferentialAction.SetNull);

		migrationBuilder.AddForeignKey(
				name: "FK_Logs_Users_AffectedUserGuid",
				schema: "public",
				table: "Logs",
				column: "AffectedUserGuid",
				principalSchema: "public",
				principalTable: "Users",
				principalColumn: "Guid",
				onDelete: ReferentialAction.SetNull);

		migrationBuilder.AddForeignKey(
				name: "FK_Logs_Users_AuthorGuid",
				schema: "public",
				table: "Logs",
				column: "AuthorGuid",
				principalSchema: "public",
				principalTable: "Users",
				principalColumn: "Guid",
				onDelete: ReferentialAction.SetNull);

		migrationBuilder.Sql(@"
					-- Для категории Source (50) -> AffectedSourceId
					UPDATE ""Logs""
					SET ""AffectedSourceId"" = NULLIF(""RefId""::INT, NULL)
					WHERE 
							""Category"" = 50
							AND ""RefId"" IS NOT NULL
							AND ""AffectedSourceId"" IS NULL
							AND ""RefId"" ~ '^\d+$' -- Проверка, является ли RefId числом
							AND EXISTS (SELECT 1 FROM ""Sources"" WHERE ""Id"" = ""RefId""::INT);

					-- Для категории Tag (60) -> AffectedTagId
					UPDATE ""Logs"" L
					SET ""AffectedTagId"" = T.""Id""
					FROM ""Tags"" T
					WHERE L.""Category"" = 60
						AND L.""RefId"" IS NOT NULL
						AND L.""AffectedTagId"" IS NULL
						AND L.""RefId"" ~ '^[0-9a-fA-F-]{36}$'
						AND T.""GlobalGuid"" = L.""RefId""::UUID;

					-- Для категории Blocks (100) -> AffectedBlockId
					UPDATE ""Logs""
					SET ""AffectedBlockId"" = NULLIF(""RefId""::INT, NULL)
					WHERE 
							""Category"" = 100
							AND ""RefId"" IS NOT NULL
							AND ""AffectedBlockId"" IS NULL
							AND ""RefId"" ~ '^\d+$'
							AND EXISTS (SELECT 1 FROM ""Blocks"" WHERE ""Id"" = ""RefId""::INT);

					-- Для категории Users (80) -> AffectedUserGuid
					UPDATE ""Logs""
					SET ""AffectedUserGuid"" = NULLIF(""RefId""::UUID, NULL)
					WHERE 
							""Category"" = 80
							AND ""RefId"" IS NOT NULL
							AND ""AffectedUserGuid"" IS NULL
							AND ""RefId"" ~ '^[0-9a-fA-F-]{36}$' -- Проверка, соответствует ли RefId формату UUID
							AND EXISTS (SELECT 1 FROM ""Users"" WHERE ""Guid"" = ""RefId""::UUID);

					-- Для категории UserGroups (90) -> AffectedUserGroupGuid
					UPDATE ""Logs""
					SET ""AffectedUserGroupGuid"" = NULLIF(""RefId""::UUID, NULL)
					WHERE 
							""Category"" = 90
							AND ""RefId"" IS NOT NULL
							AND ""AffectedUserGroupGuid"" IS NULL
							AND ""RefId"" ~ '^[0-9a-fA-F-]{36}$'
							AND EXISTS (SELECT 1 FROM ""UserGroups"" WHERE ""Guid"" = ""RefId""::UUID);");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropForeignKey(
				name: "FK_Logs_AccessRights_AffectedAccessRightsId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropForeignKey(
				name: "FK_Logs_Blocks_AffectedBlockId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropForeignKey(
				name: "FK_Logs_Sources_AffectedSourceId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropForeignKey(
				name: "FK_Logs_Tags_AffectedTagId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropForeignKey(
				name: "FK_Logs_UserGroups_AffectedUserGroupGuid",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropForeignKey(
				name: "FK_Logs_Users_AffectedUserGuid",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropForeignKey(
				name: "FK_Logs_Users_AuthorGuid",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropIndex(
				name: "IX_Logs_AffectedAccessRightsId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropIndex(
				name: "IX_Logs_AffectedBlockId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropIndex(
				name: "IX_Logs_AffectedSourceId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropIndex(
				name: "IX_Logs_AffectedTagId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropIndex(
				name: "IX_Logs_AffectedUserGroupGuid",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropIndex(
				name: "IX_Logs_AffectedUserGuid",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropColumn(
				name: "IsDeleted",
				schema: "public",
				table: "Users");

		migrationBuilder.DropColumn(
				name: "IsDeleted",
				schema: "public",
				table: "UserGroups");

		migrationBuilder.DropColumn(
				name: "IsDeleted",
				schema: "public",
				table: "Tags");

		migrationBuilder.DropColumn(
				name: "IsDeleted",
				schema: "public",
				table: "Sources");

		migrationBuilder.DropColumn(
				name: "AffectedAccessRightsId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropColumn(
				name: "AffectedBlockId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropColumn(
				name: "AffectedSourceId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropColumn(
				name: "AffectedTagId",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropColumn(
				name: "AffectedUserGroupGuid",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropColumn(
				name: "AffectedUserGuid",
				schema: "public",
				table: "Logs");

		migrationBuilder.DropColumn(
				name: "IsDeleted",
				schema: "public",
				table: "Blocks");

		migrationBuilder.RenameColumn(
				name: "AuthorGuid",
				schema: "public",
				table: "Logs",
				newName: "UserGuid");

		migrationBuilder.RenameIndex(
				name: "IX_Logs_AuthorGuid",
				schema: "public",
				table: "Logs",
				newName: "IX_Logs_UserGuid");

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
}
