using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyAccessRights : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanControlAccess",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "CanManageBlock",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "CanManageBlockTags",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "CanManageSource",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "CanManageSourceTags",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "CanManageTag",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "CanViewLogs",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "CanViewSystemTags",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "CanWriteToTag",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "HasAccessToBlock",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "HasAccessToSource",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "HasAccessToTag",
                table: "AccessRights");

            migrationBuilder.AddColumn<int>(
                name: "AccessType",
                table: "AccessRights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsGlobal",
                table: "AccessRights",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE \"Tags\" " +
                "SET \"GlobalGuid\" = gen_random_uuid() " +
                "WHERE \"GlobalGuid\" = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.Sql(
                "UPDATE \"Tags\" " +
                "SET \"Created\" = now() " +
                "WHERE \"Created\" = '-infinity';");

            migrationBuilder.Sql(
                "UPDATE \"Blocks\" " +
                "SET \"GlobalId\" = gen_random_uuid() " +
                "WHERE \"GlobalId\" = '00000000-0000-0000-0000-000000000000';");
    }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessType",
                table: "AccessRights");

            migrationBuilder.DropColumn(
                name: "IsGlobal",
                table: "AccessRights");

            migrationBuilder.AddColumn<bool>(
                name: "CanControlAccess",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageBlock",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageBlockTags",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageSource",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageSourceTags",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanManageTag",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanViewLogs",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanViewSystemTags",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CanWriteToTag",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAccessToBlock",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAccessToSource",
                table: "AccessRights",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAccessToTag",
                table: "AccessRights",
                type: "boolean",
                nullable: true);
        }
    }
}
