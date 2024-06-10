using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatalakeDatabase.Migrations
{
    /// <inheritdoc />
    public partial class AddKeycloakToUserTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Users",
                newName: "Login");

            migrationBuilder.RenameColumn(
                name: "Hash",
                table: "Users",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "UserGuid",
                table: "Users",
                newName: "Guid");

            migrationBuilder.RenameColumn(
                name: "ParentGroupGuid",
                table: "UserGroups",
                newName: "ParentGuid");

            migrationBuilder.RenameColumn(
                name: "UserGroupGuid",
                table: "UserGroups",
                newName: "Guid");

            migrationBuilder.AddColumn<Guid>(
                name: "KeycloakGuid",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KeycloakGuid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "Login",
                table: "Users",
                newName: "Hash");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "Users",
                newName: "UserGuid");

            migrationBuilder.RenameColumn(
                name: "ParentGuid",
                table: "UserGroups",
                newName: "ParentGroupGuid");

            migrationBuilder.RenameColumn(
                name: "Guid",
                table: "UserGroups",
                newName: "UserGroupGuid");
        }
    }
}
