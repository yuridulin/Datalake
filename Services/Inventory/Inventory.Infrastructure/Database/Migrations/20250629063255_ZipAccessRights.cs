using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Migrations;

/// <inheritdoc />
public partial class ZipAccessRights : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(@"
				UPDATE ""UserGroupRelation"" SET
					""AccessType"" = CASE ""AccessType""
						WHEN -100 THEN 0
						WHEN 0 THEN 4
						WHEN 5 THEN 1
						WHEN 10 THEN 2
						WHEN 50 THEN 3
						WHEN 100 THEN 5
						ELSE 0 
					END;");

		migrationBuilder.Sql(@"
				UPDATE ""AccessRights"" SET
					""AccessType"" = CASE ""AccessType""
						WHEN -100 THEN 0
						WHEN 0 THEN 4
						WHEN 5 THEN 1
						WHEN 10 THEN 2
						WHEN 50 THEN 3
						WHEN 100 THEN 5
						ELSE 0 
					END;");

		migrationBuilder.AlterColumn<byte>(
				name: "AccessType",
				schema: "public",
				table: "UserGroupRelation",
				type: "smallint",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "integer");

		migrationBuilder.AlterColumn<byte>(
				name: "AccessType",
				schema: "public",
				table: "AccessRights",
				type: "smallint",
				nullable: false,
				oldClrType: typeof(int),
				oldType: "integer");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AlterColumn<int>(
				name: "AccessType",
				schema: "public",
				table: "UserGroupRelation",
				type: "integer",
				nullable: false,
				oldClrType: typeof(byte),
				oldType: "smallint");

		migrationBuilder.AlterColumn<int>(
				name: "AccessType",
				schema: "public",
				table: "AccessRights",
				type: "integer",
				nullable: false,
				oldClrType: typeof(byte),
				oldType: "smallint");


		migrationBuilder.Sql(@"
				UPDATE ""UserGroupRelation"" SET
					""AccessType"" = CASE ""AccessType""
						WHEN 0 THEN -100
						WHEN 4 THEN 0
						WHEN 1 THEN 5
						WHEN 2 THEN 10
						WHEN 3 THEN 50
						WHEN 5 THEN 100
						ELSE 0 
					END;");

		migrationBuilder.Sql(@"
				UPDATE ""AccessRights"" SET
					""AccessType"" = CASE ""AccessType""
						WHEN 0 THEN -100
						WHEN 4 THEN 0
						WHEN 1 THEN 5
						WHEN 2 THEN 10
						WHEN 3 THEN 50
						WHEN 5 THEN 100
						ELSE 0 
					END;");
	}
}
