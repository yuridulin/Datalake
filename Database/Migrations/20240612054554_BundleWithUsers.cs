using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class BundleWithUsers : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
					name: "Hash",
					table: "Users");

			migrationBuilder.DropColumn(
					name: "Name",
					table: "Users");

			migrationBuilder.RenameColumn(
					name: "AccessType",
					table: "Users",
					newName: "Type");

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
					name: "EnergoIdGuid",
					table: "Users",
					type: "uuid",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "Login",
					table: "Users",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "PasswordHash",
					table: "Users",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "EnergoIdHost",
					table: "Settings",
					type: "text",
					nullable: false,
					defaultValue: "");

			migrationBuilder.AlterColumn<string>(
					name: "RefId",
					table: "Logs",
					type: "text",
					nullable: true,
					oldClrType: typeof(int),
					oldType: "integer",
					oldNullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
					name: "EnergoIdGuid",
					table: "Users");

			migrationBuilder.DropColumn(
					name: "Login",
					table: "Users");

			migrationBuilder.DropColumn(
					name: "PasswordHash",
					table: "Users");

			migrationBuilder.DropColumn(
					name: "EnergoIdHost",
					table: "Settings");

			migrationBuilder.RenameColumn(
					name: "Type",
					table: "Users",
					newName: "AccessType");

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

			migrationBuilder.AddColumn<string>(
					name: "Hash",
					table: "Users",
					type: "text",
					nullable: false,
					defaultValue: "");

			migrationBuilder.AddColumn<string>(
					name: "Name",
					table: "Users",
					type: "text",
					nullable: false,
					defaultValue: "");

			migrationBuilder.AlterColumn<int>(
					name: "RefId",
					table: "Logs",
					type: "integer",
					nullable: true,
					oldClrType: typeof(string),
					oldType: "text",
					oldNullable: true);
		}
	}
}
