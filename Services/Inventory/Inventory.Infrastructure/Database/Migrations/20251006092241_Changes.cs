using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class Changes : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
					name: "IX_UserGroupRelations_UserGroupGuid",
					schema: "public",
					table: "UserGroupRelations");

			migrationBuilder.DropIndex(
					name: "IX_BlockTags_BlockId",
					schema: "public",
					table: "BlockTags");

			migrationBuilder.AlterColumn<int>(
					name: "TagId",
					schema: "public",
					table: "BlockTags",
					type: "integer",
					nullable: true,
					oldClrType: typeof(int),
					oldType: "integer");

			migrationBuilder.CreateIndex(
					name: "IX_UserGroupRelations_UserGroupGuid_UserGuid",
					schema: "public",
					table: "UserGroupRelations",
					columns: new[] { "UserGroupGuid", "UserGuid" },
					unique: true);

			migrationBuilder.CreateIndex(
					name: "IX_BlockTags_BlockId_TagId",
					schema: "public",
					table: "BlockTags",
					columns: new[] { "BlockId", "TagId" },
					unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
					name: "IX_UserGroupRelations_UserGroupGuid_UserGuid",
					schema: "public",
					table: "UserGroupRelations");

			migrationBuilder.DropIndex(
					name: "IX_BlockTags_BlockId_TagId",
					schema: "public",
					table: "BlockTags");

			migrationBuilder.AlterColumn<int>(
					name: "TagId",
					schema: "public",
					table: "BlockTags",
					type: "integer",
					nullable: false,
					defaultValue: 0,
					oldClrType: typeof(int),
					oldType: "integer",
					oldNullable: true);

			migrationBuilder.CreateIndex(
					name: "IX_UserGroupRelations_UserGroupGuid",
					schema: "public",
					table: "UserGroupRelations",
					column: "UserGroupGuid");

			migrationBuilder.CreateIndex(
					name: "IX_BlockTags_BlockId",
					schema: "public",
					table: "BlockTags",
					column: "BlockId");
		}
	}
}
