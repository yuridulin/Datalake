using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations;

/// <inheritdoc />
public partial class SwitchToFluentAnnotations : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		// вот такое - из-за того, что движок не может в оптимизацию в ходе цикла и съедает все дисковое пространство
		for (int year = 2023; year <= 2026; year++)
		{
			for (int month = 1; month <= 12; month++)
			{
				for (int day = 1; day <= 31; day++)
				{
					migrationBuilder.Sql($@"
							DO $$
							BEGIN
									IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'TagsHistory_{year}_{month:D2}_{day:D2}') THEN
											ALTER TABLE ""TagsHistory_{year}_{month:D2}_{day:D2}""
											ALTER COLUMN ""Date"" TYPE timestamp USING ""Date""::timestamp;
									END IF;
							END $$;", true);
				}
			}
		}

		migrationBuilder.Sql(@"ALTER TABLE ""Logs"" ALTER COLUMN ""Date"" TYPE timestamp USING ""Date""::timestamp;", true);

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
				name: "FK_Blocks_Blocks_ParentId",
				schema: "public",
				table: "Blocks");

		migrationBuilder.DropForeignKey(
				name: "FK_BlockTags_Blocks_BlockId",
				schema: "public",
				table: "BlockTags");

		migrationBuilder.DropForeignKey(
				name: "FK_BlockTags_Tags_TagId",
				schema: "public",
				table: "BlockTags");

		migrationBuilder.DropForeignKey(
				name: "FK_TagInputs_Tags_InputTagId",
				schema: "public",
				table: "TagInputs");

		migrationBuilder.DropForeignKey(
				name: "FK_UserGroupRelation_UserGroups_UserGuid",
				schema: "public",
				table: "UserGroupRelation");

		migrationBuilder.DropForeignKey(
				name: "FK_UserGroupRelation_Users_UserGroupGuid",
				schema: "public",
				table: "UserGroupRelation");

		migrationBuilder.AlterColumn<int>(
				name: "InputTagId",
				schema: "public",
				table: "TagInputs",
				type: "integer",
				nullable: true,
				oldClrType: typeof(int),
				oldType: "integer");

		migrationBuilder.CreateIndex(
				name: "IX_UserGroups_ParentGuid",
				schema: "public",
				table: "UserGroups",
				column: "ParentGuid");

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
				name: "FK_Blocks_Blocks_ParentId",
				schema: "public",
				table: "Blocks",
				column: "ParentId",
				principalSchema: "public",
				principalTable: "Blocks",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetNull);

		migrationBuilder.AddForeignKey(
				name: "FK_BlockTags_Blocks_BlockId",
				schema: "public",
				table: "BlockTags",
				column: "BlockId",
				principalSchema: "public",
				principalTable: "Blocks",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

		migrationBuilder.AddForeignKey(
				name: "FK_BlockTags_Tags_TagId",
				schema: "public",
				table: "BlockTags",
				column: "TagId",
				principalSchema: "public",
				principalTable: "Tags",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetNull);

		migrationBuilder.AddForeignKey(
				name: "FK_TagInputs_Tags_InputTagId",
				schema: "public",
				table: "TagInputs",
				column: "InputTagId",
				principalSchema: "public",
				principalTable: "Tags",
				principalColumn: "Id",
				onDelete: ReferentialAction.SetNull);

		migrationBuilder.AddForeignKey(
				name: "FK_UserGroupRelation_UserGroups_UserGuid",
				schema: "public",
				table: "UserGroupRelation",
				column: "UserGuid",
				principalSchema: "public",
				principalTable: "UserGroups",
				principalColumn: "Guid",
				onDelete: ReferentialAction.Cascade);

		migrationBuilder.AddForeignKey(
				name: "FK_UserGroupRelation_Users_UserGroupGuid",
				schema: "public",
				table: "UserGroupRelation",
				column: "UserGroupGuid",
				principalSchema: "public",
				principalTable: "Users",
				principalColumn: "Guid",
				onDelete: ReferentialAction.Cascade);

		migrationBuilder.AddForeignKey(
				name: "FK_UserGroups_UserGroups_ParentGuid",
				schema: "public",
				table: "UserGroups",
				column: "ParentGuid",
				principalSchema: "public",
				principalTable: "UserGroups",
				principalColumn: "Guid",
				onDelete: ReferentialAction.SetNull);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
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
				name: "FK_Blocks_Blocks_ParentId",
				schema: "public",
				table: "Blocks");

		migrationBuilder.DropForeignKey(
				name: "FK_BlockTags_Blocks_BlockId",
				schema: "public",
				table: "BlockTags");

		migrationBuilder.DropForeignKey(
				name: "FK_BlockTags_Tags_TagId",
				schema: "public",
				table: "BlockTags");

		migrationBuilder.DropForeignKey(
				name: "FK_TagInputs_Tags_InputTagId",
				schema: "public",
				table: "TagInputs");

		migrationBuilder.DropForeignKey(
				name: "FK_UserGroupRelation_UserGroups_UserGuid",
				schema: "public",
				table: "UserGroupRelation");

		migrationBuilder.DropForeignKey(
				name: "FK_UserGroupRelation_Users_UserGroupGuid",
				schema: "public",
				table: "UserGroupRelation");

		migrationBuilder.DropForeignKey(
				name: "FK_UserGroups_UserGroups_ParentGuid",
				schema: "public",
				table: "UserGroups");

		migrationBuilder.DropIndex(
				name: "IX_UserGroups_ParentGuid",
				schema: "public",
				table: "UserGroups");

		migrationBuilder.AlterColumn<int>(
				name: "InputTagId",
				schema: "public",
				table: "TagInputs",
				type: "integer",
				nullable: false,
				defaultValue: 0,
				oldClrType: typeof(int),
				oldType: "integer",
				oldNullable: true);

		migrationBuilder.AddForeignKey(
				name: "FK_AccessRights_Blocks_BlockId",
				schema: "public",
				table: "AccessRights",
				column: "BlockId",
				principalSchema: "public",
				principalTable: "Blocks",
				principalColumn: "Id");

		migrationBuilder.AddForeignKey(
				name: "FK_AccessRights_Sources_SourceId",
				schema: "public",
				table: "AccessRights",
				column: "SourceId",
				principalSchema: "public",
				principalTable: "Sources",
				principalColumn: "Id");

		migrationBuilder.AddForeignKey(
				name: "FK_AccessRights_Tags_TagId",
				schema: "public",
				table: "AccessRights",
				column: "TagId",
				principalSchema: "public",
				principalTable: "Tags",
				principalColumn: "Id");

		migrationBuilder.AddForeignKey(
				name: "FK_AccessRights_UserGroups_UserGroupGuid",
				schema: "public",
				table: "AccessRights",
				column: "UserGroupGuid",
				principalSchema: "public",
				principalTable: "UserGroups",
				principalColumn: "Guid");

		migrationBuilder.AddForeignKey(
				name: "FK_AccessRights_Users_UserGuid",
				schema: "public",
				table: "AccessRights",
				column: "UserGuid",
				principalSchema: "public",
				principalTable: "Users",
				principalColumn: "Guid");

		migrationBuilder.AddForeignKey(
				name: "FK_Blocks_Blocks_ParentId",
				schema: "public",
				table: "Blocks",
				column: "ParentId",
				principalSchema: "public",
				principalTable: "Blocks",
				principalColumn: "Id");

		migrationBuilder.AddForeignKey(
				name: "FK_BlockTags_Blocks_BlockId",
				schema: "public",
				table: "BlockTags",
				column: "BlockId",
				principalSchema: "public",
				principalTable: "Blocks",
				principalColumn: "Id");

		migrationBuilder.AddForeignKey(
				name: "FK_BlockTags_Tags_TagId",
				schema: "public",
				table: "BlockTags",
				column: "TagId",
				principalSchema: "public",
				principalTable: "Tags",
				principalColumn: "Id");

		migrationBuilder.AddForeignKey(
				name: "FK_TagInputs_Tags_InputTagId",
				schema: "public",
				table: "TagInputs",
				column: "InputTagId",
				principalSchema: "public",
				principalTable: "Tags",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);

		migrationBuilder.AddForeignKey(
				name: "FK_UserGroupRelation_UserGroups_UserGuid",
				schema: "public",
				table: "UserGroupRelation",
				column: "UserGuid",
				principalSchema: "public",
				principalTable: "UserGroups",
				principalColumn: "Guid");

		migrationBuilder.AddForeignKey(
				name: "FK_UserGroupRelation_Users_UserGroupGuid",
				schema: "public",
				table: "UserGroupRelation",
				column: "UserGroupGuid",
				principalSchema: "public",
				principalTable: "Users",
				principalColumn: "Guid");
	}
}
