using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class FixRelationBetweenUsersAndGroups : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_UserGroupRelation_UserGroups_UserGuid",
					schema: "public",
					table: "UserGroupRelation");

			migrationBuilder.DropForeignKey(
					name: "FK_UserGroupRelation_Users_UserGroupGuid",
					schema: "public",
					table: "UserGroupRelation");

			migrationBuilder.RenameColumn(
				name: "UserGroupGuid",
				table: "UserGroupRelation",
				newName: "UserGuidTemp");

			migrationBuilder.RenameColumn(
				name: "UserGuid",
				table: "UserGroupRelation",
				newName: "UserGroupGuid");

			migrationBuilder.RenameColumn(
				name: "UserGuidTemp",
				table: "UserGroupRelation",
				newName: "UserGuid");

			migrationBuilder.AddForeignKey(
					name: "FK_UserGroupRelation_UserGroups_UserGroupGuid",
					schema: "public",
					table: "UserGroupRelation",
					column: "UserGroupGuid",
					principalSchema: "public",
					principalTable: "UserGroups",
					principalColumn: "Guid",
					onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
					name: "FK_UserGroupRelation_Users_UserGuid",
					schema: "public",
					table: "UserGroupRelation",
					column: "UserGuid",
					principalSchema: "public",
					principalTable: "Users",
					principalColumn: "Guid",
					onDelete: ReferentialAction.Cascade);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
					name: "FK_UserGroupRelation_UserGroups_UserGroupGuid",
					schema: "public",
					table: "UserGroupRelation");

			migrationBuilder.DropForeignKey(
					name: "FK_UserGroupRelation_Users_UserGuid",
					schema: "public",
					table: "UserGroupRelation");

			migrationBuilder.RenameColumn(
				name: "UserGroupGuid",
				table: "UserGroupRelation",
				newName: "UserGuidTemp");

			migrationBuilder.RenameColumn(
				name: "UserGuid",
				table: "UserGroupRelation",
				newName: "UserGroupGuid");

			migrationBuilder.RenameColumn(
				name: "UserGuidTemp",
				table: "UserGroupRelation",
				newName: "UserGuid");

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
		}
	}
}
