using Datalake.Contracts.Public.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class SeparateContexts : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
				WITH upd AS (
					SELECT ""UserGuid"", ""UserGroupGuid"",
									 row_number() OVER (ORDER BY ""UserGuid"", ""UserGroupGuid"") AS rn
					FROM ""UserGroupRelation""
				)
				UPDATE ""UserGroupRelation"" t
				SET ""Id"" = upd.rn
				FROM upd
				WHERE t.""UserGuid"" = upd.""UserGuid""
					AND t.""UserGroupGuid"" = upd.""UserGroupGuid"";");

			migrationBuilder.DropForeignKey(
					name: "FK_UserGroupRelation_UserGroups_UserGroupGuid",
					schema: "public",
					table: "UserGroupRelation");

			migrationBuilder.DropForeignKey(
					name: "FK_UserGroupRelation_Users_UserGuid",
					schema: "public",
					table: "UserGroupRelation");

			/*migrationBuilder.DropTable(
					name: "TagsHistory",
					schema: "public");

			migrationBuilder.DropTable(
					name: "UserSessions",
					schema: "public");*/

			/*migrationBuilder.DropPrimaryKey(
					name: "PK_BlockTags",
					schema: "public",
					table: "BlockTags");*/

			migrationBuilder.DropPrimaryKey(
					name: "PK_UserGroupRelation",
					schema: "public",
					table: "UserGroupRelation");

			migrationBuilder.RenameTable(
					name: "UserGroupRelation",
					schema: "public",
					newName: "UserGroupRelations",
					newSchema: "public");

			migrationBuilder.RenameIndex(
					name: "IX_UserGroupRelation_UserGuid",
					schema: "public",
					table: "UserGroupRelations",
					newName: "IX_UserGroupRelations_UserGuid");

			migrationBuilder.AlterColumn<string>(
					name: "Type",
					schema: "public",
					table: "Users",
					type: "character varying(20)",
					maxLength: 20,
					nullable: false,
					oldClrType: typeof(int),
					oldType: "integer");

			migrationBuilder.AlterColumn<string>(
					name: "Login",
					schema: "public",
					table: "Users",
					type: "character varying(100)",
					maxLength: 100,
					nullable: true,
					oldClrType: typeof(string),
					oldType: "text",
					oldNullable: true);

			migrationBuilder.AlterColumn<string>(
					name: "FullName",
					schema: "public",
					table: "Users",
					type: "character varying(200)",
					maxLength: 200,
					nullable: false,
					defaultValue: "",
					oldClrType: typeof(string),
					oldType: "text",
					oldNullable: true);

			migrationBuilder.AddColumn<int>(
					name: "Id",
					schema: "public",
					table: "Settings",
					type: "integer",
					nullable: false,
					defaultValue: 0)
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

			migrationBuilder.AlterColumn<int>(
					name: "Id",
					schema: "public",
					table: "UserGroupRelations",
					type: "integer",
					nullable: false,
					oldClrType: typeof(int),
					oldType: "integer")
					.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

			migrationBuilder.AddPrimaryKey(
					name: "PK_Settings",
					schema: "public",
					table: "Settings",
					column: "Id");

			migrationBuilder.AddPrimaryKey(
					name: "PK_BlockTags",
					schema: "public",
					table: "BlockTags",
					column: "Id");

			migrationBuilder.AddPrimaryKey(
					name: "PK_UserGroupRelations",
					schema: "public",
					table: "UserGroupRelations",
					column: "Id");

			migrationBuilder.CreateTable(
					name: "TagThresholds",
					schema: "public",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						TagId = table.Column<int>(type: "integer", nullable: false),
						InputValue = table.Column<float>(type: "real", nullable: false),
						OutputValue = table.Column<float>(type: "real", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_TagThresholds", x => x.Id);
						table.ForeignKey(
											name: "FK_TagThresholds_Tags_TagId",
											column: x => x.TagId,
											principalSchema: "public",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.Sql($@"
					INSERT INTO public.""Sources"" (""Id"", ""Name"", ""Type"", ""Address"", ""Description"", ""IsDeleted"", ""IsDisabled"")
					VALUES ({(int)SourceType.Thresholds}, 'Thresholds', {(int)SourceType.Thresholds}, '',
						'Теги, значения которых считаются на стороне БД как самое близкое по модулю среди пороговых значений', false, false);");
			
			migrationBuilder.Sql(@"SELECT setval(pg_get_serial_sequence('""Sources""', 'Id'), (SELECT MAX(""Id"") FROM public.""Sources""));");

			migrationBuilder.Sql(@"
					INSERT INTO ""TagThresholds"" (""TagId"", ""InputValue"", ""OutputValue"")
					SELECT 
						t.""Id"",
						(elem->>'Threshold')::REAL AS ""InputValue"",
						(elem->>'Result')::REAL AS ""OutputValue""
					FROM ""Tags"" t
					CROSS JOIN LATERAL jsonb_array_elements(t.""Thresholds"") AS elem
					WHERE t.""Thresholds"" IS NOT NULL;");

			migrationBuilder.Sql($@"
					UPDATE ""Tags"" SET ""SourceId"" = {(int)SourceType.Thresholds}
					WHERE ""Id"" IN (
						SELECT tt.""TagId""
						FROM ""TagThresholds"" tt
						GROUP BY tt.""TagId"")");

			migrationBuilder.DropColumn(
					name: "Calculation",
					schema: "public",
					table: "Tags");

			migrationBuilder.DropColumn(
					name: "Thresholds",
					schema: "public",
					table: "Tags");

			migrationBuilder.CreateIndex(
					name: "IX_Users_Login_EnergoIdGuid",
					schema: "public",
					table: "Users",
					columns: new[] { "Login", "EnergoIdGuid" },
					unique: true);

			migrationBuilder.CreateIndex(
					name: "IX_Users_Type",
					schema: "public",
					table: "Users",
					column: "Type");

			migrationBuilder.CreateIndex(
					name: "IX_BlockTags_BlockId",
					schema: "public",
					table: "BlockTags",
					column: "BlockId");

			migrationBuilder.CreateIndex(
					name: "IX_UserGroupRelations_UserGroupGuid",
					schema: "public",
					table: "UserGroupRelations",
					column: "UserGroupGuid");

			migrationBuilder.CreateIndex(
					name: "IX_TagThresholds_TagId",
					schema: "public",
					table: "TagThresholds",
					column: "TagId");

			migrationBuilder.AddForeignKey(
					name: "FK_UserGroupRelations_UserGroups_UserGroupGuid",
					schema: "public",
					table: "UserGroupRelations",
					column: "UserGroupGuid",
					principalSchema: "public",
					principalTable: "UserGroups",
					principalColumn: "Guid",
					onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
					name: "FK_UserGroupRelations_Users_UserGuid",
					schema: "public",
					table: "UserGroupRelations",
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
					name: "FK_UserGroupRelations_UserGroups_UserGroupGuid",
					schema: "public",
					table: "UserGroupRelations");

			migrationBuilder.DropForeignKey(
					name: "FK_UserGroupRelations_Users_UserGuid",
					schema: "public",
					table: "UserGroupRelations");

			migrationBuilder.DropTable(
					name: "TagThresholds",
					schema: "public");

			migrationBuilder.DropIndex(
					name: "IX_Users_Login_EnergoIdGuid",
					schema: "public",
					table: "Users");

			migrationBuilder.DropIndex(
					name: "IX_Users_Type",
					schema: "public",
					table: "Users");

			migrationBuilder.DropPrimaryKey(
					name: "PK_Settings",
					schema: "public",
					table: "Settings");

			migrationBuilder.DropPrimaryKey(
					name: "PK_BlockTags",
					schema: "public",
					table: "BlockTags");

			migrationBuilder.DropIndex(
					name: "IX_BlockTags_BlockId",
					schema: "public",
					table: "BlockTags");

			migrationBuilder.DropPrimaryKey(
					name: "PK_UserGroupRelations",
					schema: "public",
					table: "UserGroupRelations");

			migrationBuilder.DropIndex(
					name: "IX_UserGroupRelations_UserGroupGuid",
					schema: "public",
					table: "UserGroupRelations");

			migrationBuilder.DropColumn(
					name: "Id",
					schema: "public",
					table: "Settings");

			migrationBuilder.RenameTable(
					name: "UserGroupRelations",
					schema: "public",
					newName: "UserGroupRelation",
					newSchema: "public");

			migrationBuilder.RenameIndex(
					name: "IX_UserGroupRelations_UserGuid",
					schema: "public",
					table: "UserGroupRelation",
					newName: "IX_UserGroupRelation_UserGuid");

			migrationBuilder.AlterColumn<int>(
					name: "Type",
					schema: "public",
					table: "Users",
					type: "integer",
					nullable: false,
					oldClrType: typeof(string),
					oldType: "character varying(20)",
					oldMaxLength: 20);

			migrationBuilder.AlterColumn<string>(
					name: "Login",
					schema: "public",
					table: "Users",
					type: "text",
					nullable: true,
					oldClrType: typeof(string),
					oldType: "character varying(100)",
					oldMaxLength: 100,
					oldNullable: true);

			migrationBuilder.AlterColumn<string>(
					name: "FullName",
					schema: "public",
					table: "Users",
					type: "text",
					nullable: true,
					oldClrType: typeof(string),
					oldType: "character varying(200)",
					oldMaxLength: 200);

			migrationBuilder.AddColumn<int>(
					name: "Calculation",
					schema: "public",
					table: "Tags",
					type: "integer",
					nullable: true);

			migrationBuilder.AddColumn<string>(
					name: "Thresholds",
					schema: "public",
					table: "Tags",
					type: "jsonb",
					nullable: true);

			migrationBuilder.AlterColumn<int>(
					name: "Id",
					schema: "public",
					table: "UserGroupRelation",
					type: "integer",
					nullable: false,
					oldClrType: typeof(int),
					oldType: "integer")
					.OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

			migrationBuilder.AddPrimaryKey(
					name: "PK_BlockTags",
					schema: "public",
					table: "BlockTags",
					columns: new[] { "BlockId", "TagId" });

			migrationBuilder.AddPrimaryKey(
					name: "PK_UserGroupRelation",
					schema: "public",
					table: "UserGroupRelation",
					columns: new[] { "UserGroupGuid", "UserGuid" });

			migrationBuilder.CreateTable(
					name: "TagsHistory",
					schema: "public",
					columns: table => new
					{
						Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
						Number = table.Column<float>(type: "real", nullable: true),
						Quality = table.Column<int>(type: "integer", nullable: false),
						TagId = table.Column<int>(type: "integer", nullable: false),
						Text = table.Column<string>(type: "text", nullable: true)
					},
					constraints: table =>
					{
					});

			migrationBuilder.CreateTable(
					name: "UserSessions",
					schema: "public",
					columns: table => new
					{
						UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
						Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
						ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
						Token = table.Column<string>(type: "text", nullable: false),
						Type = table.Column<int>(type: "integer", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_UserSessions", x => x.UserGuid);
						table.ForeignKey(
											name: "FK_UserSessions_Users_UserGuid",
											column: x => x.UserGuid,
											principalSchema: "public",
											principalTable: "Users",
											principalColumn: "Guid",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateIndex(
					name: "TagsHistory_TagId_Date_idx",
					schema: "public",
					table: "TagsHistory",
					columns: new[] { "TagId", "Date" },
					unique: true,
					descending: new[] { false, true });

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
	}
}
