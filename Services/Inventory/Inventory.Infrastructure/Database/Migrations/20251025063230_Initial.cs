using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class Initial : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
					name: "inventory");

			migrationBuilder.CreateTable(
					name: "Blocks",
					schema: "inventory",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						GlobalId = table.Column<Guid>(type: "uuid", nullable: false),
						ParentId = table.Column<int>(type: "integer", nullable: true),
						Name = table.Column<string>(type: "text", nullable: false),
						Description = table.Column<string>(type: "text", nullable: true),
						IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_Blocks", x => x.Id);
						table.ForeignKey(
											name: "FK_Blocks_Blocks_ParentId",
											column: x => x.ParentId,
											principalSchema: "inventory",
											principalTable: "Blocks",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
					});

			migrationBuilder.CreateTable(
					name: "Settings",
					schema: "inventory",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						LastUpdate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
						KeycloakHost = table.Column<string>(type: "text", nullable: false),
						KeycloakClient = table.Column<string>(type: "text", nullable: false),
						EnergoIdApi = table.Column<string>(type: "text", nullable: false),
						InstanceName = table.Column<string>(type: "text", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_Settings", x => x.Id);
					});

			migrationBuilder.CreateTable(
					name: "Sources",
					schema: "inventory",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						Name = table.Column<string>(type: "text", nullable: false),
						Description = table.Column<string>(type: "text", nullable: true),
						Type = table.Column<int>(type: "integer", nullable: false),
						Address = table.Column<string>(type: "text", nullable: true),
						IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
						IsDisabled = table.Column<bool>(type: "boolean", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_Sources", x => x.Id);
					});

			migrationBuilder.CreateTable(
					name: "UserGroups",
					schema: "inventory",
					columns: table => new
					{
						Guid = table.Column<Guid>(type: "uuid", nullable: false),
						ParentGuid = table.Column<Guid>(type: "uuid", nullable: true),
						Name = table.Column<string>(type: "text", nullable: false),
						Description = table.Column<string>(type: "text", nullable: true),
						IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_UserGroups", x => x.Guid);
						table.ForeignKey(
											name: "FK_UserGroups_UserGroups_ParentGuid",
											column: x => x.ParentGuid,
											principalSchema: "inventory",
											principalTable: "UserGroups",
											principalColumn: "Guid",
											onDelete: ReferentialAction.SetNull);
					});

			migrationBuilder.CreateTable(
					name: "Users",
					schema: "inventory",
					columns: table => new
					{
						Guid = table.Column<Guid>(type: "uuid", nullable: false),
						Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
						IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
						Email = table.Column<string>(type: "text", nullable: true),
						Login = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
						PasswordHash = table.Column<string>(type: "text", nullable: true),
						FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_Users", x => x.Guid);
					});

			migrationBuilder.CreateTable(
					name: "BlockProperties",
					schema: "inventory",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						BlockId = table.Column<int>(type: "integer", nullable: false),
						Name = table.Column<string>(type: "text", nullable: false),
						Type = table.Column<int>(type: "integer", nullable: false),
						Value = table.Column<string>(type: "text", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_BlockProperties", x => x.Id);
						table.ForeignKey(
											name: "FK_BlockProperties_Blocks_BlockId",
											column: x => x.BlockId,
											principalSchema: "inventory",
											principalTable: "Blocks",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateTable(
					name: "Tags",
					schema: "inventory",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						GlobalGuid = table.Column<Guid>(type: "uuid", nullable: false),
						Name = table.Column<string>(type: "text", nullable: false),
						Description = table.Column<string>(type: "text", nullable: true),
						Type = table.Column<int>(type: "integer", nullable: false),
						Resolution = table.Column<int>(type: "integer", nullable: false),
						Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
						IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
						SourceId = table.Column<int>(type: "integer", nullable: false),
						SourceItem = table.Column<string>(type: "text", nullable: true),
						IsScaling = table.Column<bool>(type: "boolean", nullable: false),
						MinEu = table.Column<float>(type: "real", nullable: false),
						MaxEu = table.Column<float>(type: "real", nullable: false),
						MinRaw = table.Column<float>(type: "real", nullable: false),
						MaxRaw = table.Column<float>(type: "real", nullable: false),
						Formula = table.Column<string>(type: "text", nullable: true),
						ThresholdSourceTagId = table.Column<int>(type: "integer", nullable: true),
						ThresholdSourceTagBlockId = table.Column<int>(type: "integer", nullable: true),
						Aggregation = table.Column<int>(type: "integer", nullable: true),
						AggregationPeriod = table.Column<int>(type: "integer", nullable: true),
						SourceTagId = table.Column<int>(type: "integer", nullable: true),
						SourceTagBlockId = table.Column<int>(type: "integer", nullable: true)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_Tags", x => x.Id);
						table.ForeignKey(
											name: "FK_Tags_Sources_SourceId",
											column: x => x.SourceId,
											principalSchema: "inventory",
											principalTable: "Sources",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_Tags_Tags_SourceTagId",
											column: x => x.SourceTagId,
											principalSchema: "inventory",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_Tags_Tags_ThresholdSourceTagId",
											column: x => x.ThresholdSourceTagId,
											principalSchema: "inventory",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
					});

			migrationBuilder.CreateTable(
					name: "UserGroupRelations",
					schema: "inventory",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
						UserGroupGuid = table.Column<Guid>(type: "uuid", nullable: false),
						AccessType = table.Column<byte>(type: "smallint", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_UserGroupRelations", x => x.Id);
						table.ForeignKey(
											name: "FK_UserGroupRelations_UserGroups_UserGroupGuid",
											column: x => x.UserGroupGuid,
											principalSchema: "inventory",
											principalTable: "UserGroups",
											principalColumn: "Guid",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_UserGroupRelations_Users_UserGuid",
											column: x => x.UserGuid,
											principalSchema: "inventory",
											principalTable: "Users",
											principalColumn: "Guid",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateTable(
					name: "AccessRules",
					schema: "inventory",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						UserGuid = table.Column<Guid>(type: "uuid", nullable: true),
						UserGroupGuid = table.Column<Guid>(type: "uuid", nullable: true),
						IsGlobal = table.Column<bool>(type: "boolean", nullable: false),
						TagId = table.Column<int>(type: "integer", nullable: true),
						SourceId = table.Column<int>(type: "integer", nullable: true),
						BlockId = table.Column<int>(type: "integer", nullable: true),
						AccessType = table.Column<byte>(type: "smallint", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_AccessRules", x => x.Id);
						table.ForeignKey(
											name: "FK_AccessRules_Blocks_BlockId",
											column: x => x.BlockId,
											principalSchema: "inventory",
											principalTable: "Blocks",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_AccessRules_Sources_SourceId",
											column: x => x.SourceId,
											principalSchema: "inventory",
											principalTable: "Sources",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_AccessRules_Tags_TagId",
											column: x => x.TagId,
											principalSchema: "inventory",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_AccessRules_UserGroups_UserGroupGuid",
											column: x => x.UserGroupGuid,
											principalSchema: "inventory",
											principalTable: "UserGroups",
											principalColumn: "Guid",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_AccessRules_Users_UserGuid",
											column: x => x.UserGuid,
											principalSchema: "inventory",
											principalTable: "Users",
											principalColumn: "Guid",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateTable(
					name: "BlockTags",
					schema: "inventory",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						BlockId = table.Column<int>(type: "integer", nullable: false),
						TagId = table.Column<int>(type: "integer", nullable: true),
						Name = table.Column<string>(type: "text", nullable: true),
						Relation = table.Column<int>(type: "integer", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_BlockTags", x => x.Id);
						table.ForeignKey(
											name: "FK_BlockTags_Blocks_BlockId",
											column: x => x.BlockId,
											principalSchema: "inventory",
											principalTable: "Blocks",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_BlockTags_Tags_TagId",
											column: x => x.TagId,
											principalSchema: "inventory",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
					});

			migrationBuilder.CreateTable(
					name: "CalculatedAccessRules",
					schema: "inventory",
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
											name: "FK_CalculatedAccessRules_Blocks_BlockId",
											column: x => x.BlockId,
											principalSchema: "inventory",
											principalTable: "Blocks",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_CalculatedAccessRules_Sources_SourceId",
											column: x => x.SourceId,
											principalSchema: "inventory",
											principalTable: "Sources",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_CalculatedAccessRules_Tags_TagId",
											column: x => x.TagId,
											principalSchema: "inventory",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_CalculatedAccessRules_UserGroups_UserGroupGuid",
											column: x => x.UserGroupGuid,
											principalSchema: "inventory",
											principalTable: "UserGroups",
											principalColumn: "Guid",
											onDelete: ReferentialAction.Cascade);
						table.ForeignKey(
											name: "FK_CalculatedAccessRules_Users_UserGuid",
											column: x => x.UserGuid,
											principalSchema: "inventory",
											principalTable: "Users",
											principalColumn: "Guid",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateTable(
					name: "TagInputs",
					schema: "inventory",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						TagId = table.Column<int>(type: "integer", nullable: false),
						InputTagId = table.Column<int>(type: "integer", nullable: true),
						InputBlockId = table.Column<int>(type: "integer", nullable: true),
						VariableName = table.Column<string>(type: "text", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_TagInputs", x => x.Id);
						table.ForeignKey(
											name: "FK_TagInputs_Tags_InputTagId",
											column: x => x.InputTagId,
											principalSchema: "inventory",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_TagInputs_Tags_TagId",
											column: x => x.TagId,
											principalSchema: "inventory",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateTable(
					name: "TagThresholds",
					schema: "inventory",
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
											principalSchema: "inventory",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateTable(
					name: "Logs",
					schema: "inventory",
					columns: table => new
					{
						Id = table.Column<long>(type: "bigint", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
						Category = table.Column<int>(type: "integer", nullable: false),
						AffectedSourceId = table.Column<int>(type: "integer", nullable: true),
						AffectedTagId = table.Column<int>(type: "integer", nullable: true),
						AffectedBlockId = table.Column<int>(type: "integer", nullable: true),
						AffectedUserGuid = table.Column<Guid>(type: "uuid", nullable: true),
						AffectedUserGroupGuid = table.Column<Guid>(type: "uuid", nullable: true),
						AffectedAccessRightsId = table.Column<int>(type: "integer", nullable: true),
						RefId = table.Column<string>(type: "text", nullable: true),
						AuthorGuid = table.Column<Guid>(type: "uuid", nullable: true),
						Type = table.Column<int>(type: "integer", nullable: false),
						Text = table.Column<string>(type: "text", nullable: false),
						Details = table.Column<string>(type: "text", nullable: true)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_Logs", x => x.Id);
						table.ForeignKey(
											name: "FK_Logs_AccessRules_AffectedAccessRightsId",
											column: x => x.AffectedAccessRightsId,
											principalSchema: "inventory",
											principalTable: "AccessRules",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_Logs_Blocks_AffectedBlockId",
											column: x => x.AffectedBlockId,
											principalSchema: "inventory",
											principalTable: "Blocks",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_Logs_Sources_AffectedSourceId",
											column: x => x.AffectedSourceId,
											principalSchema: "inventory",
											principalTable: "Sources",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_Logs_Tags_AffectedTagId",
											column: x => x.AffectedTagId,
											principalSchema: "inventory",
											principalTable: "Tags",
											principalColumn: "Id",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_Logs_UserGroups_AffectedUserGroupGuid",
											column: x => x.AffectedUserGroupGuid,
											principalSchema: "inventory",
											principalTable: "UserGroups",
											principalColumn: "Guid",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_Logs_Users_AffectedUserGuid",
											column: x => x.AffectedUserGuid,
											principalSchema: "inventory",
											principalTable: "Users",
											principalColumn: "Guid",
											onDelete: ReferentialAction.SetNull);
						table.ForeignKey(
											name: "FK_Logs_Users_AuthorGuid",
											column: x => x.AuthorGuid,
											principalSchema: "inventory",
											principalTable: "Users",
											principalColumn: "Guid",
											onDelete: ReferentialAction.SetNull);
					});

			migrationBuilder.CreateIndex(
					name: "IX_AccessRules_BlockId",
					schema: "inventory",
					table: "AccessRules",
					column: "BlockId");

			migrationBuilder.CreateIndex(
					name: "IX_AccessRules_SourceId",
					schema: "inventory",
					table: "AccessRules",
					column: "SourceId");

			migrationBuilder.CreateIndex(
					name: "IX_AccessRules_TagId",
					schema: "inventory",
					table: "AccessRules",
					column: "TagId");

			migrationBuilder.CreateIndex(
					name: "IX_AccessRules_UserGroupGuid",
					schema: "inventory",
					table: "AccessRules",
					column: "UserGroupGuid");

			migrationBuilder.CreateIndex(
					name: "IX_AccessRules_UserGuid",
					schema: "inventory",
					table: "AccessRules",
					column: "UserGuid");

			migrationBuilder.CreateIndex(
					name: "IX_BlockProperties_BlockId",
					schema: "inventory",
					table: "BlockProperties",
					column: "BlockId");

			migrationBuilder.CreateIndex(
					name: "IX_Blocks_ParentId",
					schema: "inventory",
					table: "Blocks",
					column: "ParentId");

			migrationBuilder.CreateIndex(
					name: "IX_BlockTags_BlockId_TagId",
					schema: "inventory",
					table: "BlockTags",
					columns: new[] { "BlockId", "TagId" },
					unique: true);

			migrationBuilder.CreateIndex(
					name: "IX_BlockTags_TagId",
					schema: "inventory",
					table: "BlockTags",
					column: "TagId");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_BlockId",
					schema: "inventory",
					table: "CalculatedAccessRules",
					column: "BlockId");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_SourceId",
					schema: "inventory",
					table: "CalculatedAccessRules",
					column: "SourceId");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_TagId",
					schema: "inventory",
					table: "CalculatedAccessRules",
					column: "TagId");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_UserGroupGuid",
					schema: "inventory",
					table: "CalculatedAccessRules",
					column: "UserGroupGuid");

			migrationBuilder.CreateIndex(
					name: "IX_CalculatedAccessRules_UserGuid_IsGlobal_BlockId_TagId_Sourc~",
					schema: "inventory",
					table: "CalculatedAccessRules",
					columns: new[] { "UserGuid", "IsGlobal", "BlockId", "TagId", "SourceId", "UserGroupGuid" },
					unique: true);

			migrationBuilder.CreateIndex(
					name: "IX_Logs_AffectedAccessRightsId",
					schema: "inventory",
					table: "Logs",
					column: "AffectedAccessRightsId");

			migrationBuilder.CreateIndex(
					name: "IX_Logs_AffectedBlockId",
					schema: "inventory",
					table: "Logs",
					column: "AffectedBlockId");

			migrationBuilder.CreateIndex(
					name: "IX_Logs_AffectedSourceId",
					schema: "inventory",
					table: "Logs",
					column: "AffectedSourceId");

			migrationBuilder.CreateIndex(
					name: "IX_Logs_AffectedTagId",
					schema: "inventory",
					table: "Logs",
					column: "AffectedTagId");

			migrationBuilder.CreateIndex(
					name: "IX_Logs_AffectedUserGroupGuid",
					schema: "inventory",
					table: "Logs",
					column: "AffectedUserGroupGuid");

			migrationBuilder.CreateIndex(
					name: "IX_Logs_AffectedUserGuid",
					schema: "inventory",
					table: "Logs",
					column: "AffectedUserGuid");

			migrationBuilder.CreateIndex(
					name: "IX_Logs_AuthorGuid",
					schema: "inventory",
					table: "Logs",
					column: "AuthorGuid");

			migrationBuilder.CreateIndex(
					name: "IX_TagInputs_InputTagId",
					schema: "inventory",
					table: "TagInputs",
					column: "InputTagId");

			migrationBuilder.CreateIndex(
					name: "IX_TagInputs_TagId",
					schema: "inventory",
					table: "TagInputs",
					column: "TagId");

			migrationBuilder.CreateIndex(
					name: "IX_Tags_SourceId",
					schema: "inventory",
					table: "Tags",
					column: "SourceId");

			migrationBuilder.CreateIndex(
					name: "IX_Tags_SourceTagId",
					schema: "inventory",
					table: "Tags",
					column: "SourceTagId");

			migrationBuilder.CreateIndex(
					name: "IX_Tags_ThresholdSourceTagId",
					schema: "inventory",
					table: "Tags",
					column: "ThresholdSourceTagId");

			migrationBuilder.CreateIndex(
					name: "IX_TagThresholds_TagId",
					schema: "inventory",
					table: "TagThresholds",
					column: "TagId");

			migrationBuilder.CreateIndex(
					name: "IX_UserGroupRelations_UserGroupGuid_UserGuid",
					schema: "inventory",
					table: "UserGroupRelations",
					columns: new[] { "UserGroupGuid", "UserGuid" },
					unique: true);

			migrationBuilder.CreateIndex(
					name: "IX_UserGroupRelations_UserGuid",
					schema: "inventory",
					table: "UserGroupRelations",
					column: "UserGuid");

			migrationBuilder.CreateIndex(
					name: "IX_UserGroups_ParentGuid",
					schema: "inventory",
					table: "UserGroups",
					column: "ParentGuid");

			migrationBuilder.CreateIndex(
					name: "IX_Users_Type",
					schema: "inventory",
					table: "Users",
					column: "Type");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
					name: "BlockProperties",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "BlockTags",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "CalculatedAccessRules",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "Logs",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "Settings",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "TagInputs",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "TagThresholds",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "UserGroupRelations",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "AccessRules",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "Blocks",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "Tags",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "UserGroups",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "Users",
					schema: "inventory");

			migrationBuilder.DropTable(
					name: "Sources",
					schema: "inventory");
		}
	}
}
