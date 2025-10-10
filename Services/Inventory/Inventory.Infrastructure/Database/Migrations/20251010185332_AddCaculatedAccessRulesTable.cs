using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddCaculatedAccessRulesTable : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
					name: "CalculatedAccessRules",
					schema: "public",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
						AccessType = table.Column<byte>(type: "smallint", nullable: false),
						IsGlobal = table.Column<bool>(type: "boolean", nullable: false),
						TagId = table.Column<int>(type: "integer", nullable: true),
						BlockId = table.Column<int>(type: "integer", nullable: true),
						SourceId = table.Column<int>(type: "integer", nullable: true),
						UserGroupGuid = table.Column<Guid>(type: "uuid", nullable: true),
						RuleId = table.Column<int>(type: "integer", nullable: false),
						UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_CalculatedAccessRules", x => x.Id);
						table.ForeignKey(
											name: "FK_CalculatedAccessRules_AccessRights_RuleId",
											column: x => x.RuleId,
											principalSchema: "public",
											principalTable: "AccessRights",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_CalculatedAccessRules_Blocks_BlockId",
											column: x => x.BlockId,
											principalSchema: "public",
											principalTable: "Blocks",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_CalculatedAccessRules_Sources_SourceId",
											column: x => x.SourceId,
											principalSchema: "public",
											principalTable: "Sources",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_CalculatedAccessRules_Tags_TagId",
											column: x => x.TagId,
											principalSchema: "public",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_CalculatedAccessRules_UserGroups_UserGroupGuid",
											column: x => x.UserGroupGuid,
											principalSchema: "public",
											principalTable: "UserGroups",
											principalColumn: "Guid",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_CalculatedAccessRules_Users_UserGuid",
											column: x => x.UserGuid,
											principalSchema: "public",
											principalTable: "Users",
											principalColumn: "Guid",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_BlockId",
					schema: "public",
					table: "CalculatedAccessRules",
					column: "BlockId");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_RuleId",
					schema: "public",
					table: "CalculatedAccessRules",
					column: "RuleId");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_SourceId",
					schema: "public",
					table: "CalculatedAccessRules",
					column: "SourceId");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_TagId",
					schema: "public",
					table: "CalculatedAccessRules",
					column: "TagId");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_UserGroupGuid",
					schema: "public",
					table: "CalculatedAccessRules",
					column: "UserGroupGuid");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_UserGuid_BlockId_TagId_SourceId_UserG~",
					schema: "public",
					table: "CalculatedAccessRules",
					columns: new[] { "UserGuid", "BlockId", "TagId", "SourceId", "UserGroupGuid" },
					unique: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
					name: "CalculatedAccessRules",
					schema: "public");
		}
	}
}
