using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddAccessRights : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropPrimaryKey(
				name: "PK_Users",
				table: "Users");

		migrationBuilder.RenameColumn(
				name: "GlobalId",
				table: "Tags",
				newName: "GlobalGuid");

		migrationBuilder.AlterColumn<string>(
				name: "FullName",
				table: "Users",
				type: "text",
				nullable: true,
				oldClrType: typeof(string),
				oldType: "text");

		migrationBuilder.AddColumn<Guid>(
				name: "UserGuid",
				table: "Users",
				type: "uuid",
				nullable: false,
				defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

		migrationBuilder.Sql(
				"UPDATE \"Users\" " +
				"SET \"UserGuid\" = gen_random_uuid();");

		migrationBuilder.AddPrimaryKey(
				name: "PK_Users",
				table: "Users",
				column: "UserGuid");

		migrationBuilder.CreateTable(
				name: "UserGroups",
				columns: table => new
				{
					UserGroupGuid = table.Column<Guid>(type: "uuid", nullable: false),
					ParentGroupGuid = table.Column<Guid>(type: "uuid", nullable: true),
					Name = table.Column<string>(type: "text", nullable: false),
					Description = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserGroups", x => x.UserGroupGuid);
				});

		migrationBuilder.CreateTable(
				name: "AccessRights",
				columns: table => new
				{
					Id = table.Column<int>(type: "integer", nullable: false)
								.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					UserGuid = table.Column<Guid>(type: "uuid", nullable: true),
					UserGroupGuid = table.Column<Guid>(type: "uuid", nullable: true),
					TagId = table.Column<int>(type: "integer", nullable: true),
					SourceId = table.Column<int>(type: "integer", nullable: true),
					BlockId = table.Column<int>(type: "integer", nullable: true),
					HasAccessToTag = table.Column<bool>(type: "boolean", nullable: true),
					CanManageTag = table.Column<bool>(type: "boolean", nullable: true),
					CanWriteToTag = table.Column<bool>(type: "boolean", nullable: true),
					HasAccessToBlock = table.Column<bool>(type: "boolean", nullable: true),
					CanManageBlock = table.Column<bool>(type: "boolean", nullable: true),
					CanManageBlockTags = table.Column<bool>(type: "boolean", nullable: true),
					HasAccessToSource = table.Column<bool>(type: "boolean", nullable: true),
					CanManageSource = table.Column<bool>(type: "boolean", nullable: true),
					CanManageSourceTags = table.Column<bool>(type: "boolean", nullable: true),
					CanControlAccess = table.Column<bool>(type: "boolean", nullable: true),
					CanViewSystemTags = table.Column<bool>(type: "boolean", nullable: true),
					CanViewLogs = table.Column<bool>(type: "boolean", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_AccessRights", x => x.Id);
					table.ForeignKey(
										name: "FK_AccessRights_Blocks_BlockId",
										column: x => x.BlockId,
										principalTable: "Blocks",
										principalColumn: "Id");
					table.ForeignKey(
										name: "FK_AccessRights_Sources_SourceId",
										column: x => x.SourceId,
										principalTable: "Sources",
										principalColumn: "Id");
					table.ForeignKey(
										name: "FK_AccessRights_Tags_TagId",
										column: x => x.TagId,
										principalTable: "Tags",
										principalColumn: "Id");
					table.ForeignKey(
										name: "FK_AccessRights_UserGroups_UserGroupGuid",
										column: x => x.UserGroupGuid,
										principalTable: "UserGroups",
										principalColumn: "UserGroupGuid");
					table.ForeignKey(
										name: "FK_AccessRights_Users_UserGuid",
										column: x => x.UserGuid,
										principalTable: "Users",
										principalColumn: "UserGuid");
				});

		migrationBuilder.CreateTable(
				name: "UserGroupRelation",
				columns: table => new
				{
					UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
					UserGroupGuid = table.Column<Guid>(type: "uuid", nullable: false),
					AccessType = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserGroupRelation", x => new { x.UserGroupGuid, x.UserGuid });
					table.ForeignKey(
										name: "FK_UserGroupRelation_UserGroups_UserGuid",
										column: x => x.UserGuid,
										principalTable: "UserGroups",
										principalColumn: "UserGroupGuid");
					table.ForeignKey(
										name: "FK_UserGroupRelation_Users_UserGroupGuid",
										column: x => x.UserGroupGuid,
										principalTable: "Users",
										principalColumn: "UserGuid");
				});

		migrationBuilder.CreateIndex(
				name: "IX_AccessRights_BlockId",
				table: "AccessRights",
				column: "BlockId");

		migrationBuilder.CreateIndex(
				name: "IX_AccessRights_SourceId",
				table: "AccessRights",
				column: "SourceId");

		migrationBuilder.CreateIndex(
				name: "IX_AccessRights_TagId",
				table: "AccessRights",
				column: "TagId");

		migrationBuilder.CreateIndex(
				name: "IX_AccessRights_UserGroupGuid",
				table: "AccessRights",
				column: "UserGroupGuid");

		migrationBuilder.CreateIndex(
				name: "IX_AccessRights_UserGuid",
				table: "AccessRights",
				column: "UserGuid");

		migrationBuilder.CreateIndex(
				name: "IX_UserGroupRelation_UserGuid",
				table: "UserGroupRelation",
				column: "UserGuid");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
				name: "AccessRights");

		migrationBuilder.DropTable(
				name: "UserGroupRelation");

		migrationBuilder.DropTable(
				name: "UserGroups");

		migrationBuilder.DropPrimaryKey(
				name: "PK_Users",
				table: "Users");

		migrationBuilder.DropColumn(
				name: "UserGuid",
				table: "Users");

		migrationBuilder.RenameColumn(
				name: "GlobalGuid",
				table: "Tags",
				newName: "GlobalId");

		migrationBuilder.AlterColumn<string>(
				name: "FullName",
				table: "Users",
				type: "text",
				nullable: false,
				defaultValue: "",
				oldClrType: typeof(string),
				oldType: "text",
				oldNullable: true);

		migrationBuilder.AddPrimaryKey(
				name: "PK_Users",
				table: "Users",
				column: "Name");
	}
}
