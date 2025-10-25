using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class RemoveStaticUsers : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
					name: "IX_Users_EnergoIdGuid",
					schema: "inventory",
					table: "Users");

			migrationBuilder.DropIndex(
					name: "IX_Users_Login_EnergoIdGuid",
					schema: "inventory",
					table: "Users");

			migrationBuilder.DropColumn(
					name: "EnergoIdGuid",
					schema: "inventory",
					table: "Users");

			migrationBuilder.RenameColumn(
					name: "StaticHost",
					schema: "inventory",
					table: "Users",
					newName: "Email");

			migrationBuilder.AlterColumn<string>(
					name: "FullName",
					schema: "inventory",
					table: "Users",
					type: "character varying(200)",
					maxLength: 200,
					nullable: true,
					oldClrType: typeof(string),
					oldType: "character varying(200)",
					oldMaxLength: 200);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
					name: "Email",
					schema: "inventory",
					table: "Users",
					newName: "StaticHost");

			migrationBuilder.AlterColumn<string>(
					name: "FullName",
					schema: "inventory",
					table: "Users",
					type: "character varying(200)",
					maxLength: 200,
					nullable: false,
					defaultValue: "",
					oldClrType: typeof(string),
					oldType: "character varying(200)",
					oldMaxLength: 200,
					oldNullable: true);

			migrationBuilder.AddColumn<Guid>(
					name: "EnergoIdGuid",
					schema: "inventory",
					table: "Users",
					type: "uuid",
					nullable: true);

			migrationBuilder.CreateIndex(
					name: "IX_Users_EnergoIdGuid",
					schema: "inventory",
					table: "Users",
					column: "EnergoIdGuid",
					unique: true);

			migrationBuilder.CreateIndex(
					name: "IX_Users_Login_EnergoIdGuid",
					schema: "inventory",
					table: "Users",
					columns: new[] { "Login", "EnergoIdGuid" },
					unique: true);
		}
	}
}
