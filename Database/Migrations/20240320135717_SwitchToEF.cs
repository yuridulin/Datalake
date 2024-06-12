using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;


#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class SwitchToEF : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"DO $$
					DECLARE
							_tbl text;
							_column_to_drop text = 'Type';
					BEGIN
							FOR _tbl IN (
									SELECT table_name
									FROM information_schema.tables
									WHERE lower(table_name) LIKE 'tagshistory_%'
							)
							LOOP
									EXECUTE format('ALTER TABLE %I DROP COLUMN IF EXISTS %I', _tbl, _column_to_drop);
							END LOOP;
					END $$;");

			// изменения схемы

			migrationBuilder.DropColumn(
					name: "PropertiesRaw",
					table: "Blocks");

			migrationBuilder.DropTable(
					name: "Logs");

			migrationBuilder.RenameTable(
					name: "Rel_Block_Tag",
					newName: "BlockTags");

			migrationBuilder.RenameTable(
					name: "Rel_Tag_Input",
					newName: "TagInputs");

			migrationBuilder.DropColumn(
					name: "Key",
					table: "Settings");

			migrationBuilder.DropColumn(
					name: "Value",
					table: "Settings");

			migrationBuilder.RenameColumn(
					name: "MinEU",
					table: "Tags",
					newName: "MinEu");

			migrationBuilder.RenameColumn(
					name: "MaxEU",
					table: "Tags",
					newName: "MaxEu");

			migrationBuilder.AddColumn<DateTime>(
					name: "Created",
					table: "Tags",
					type: "timestamp with time zone",
					nullable: false,
					defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

			migrationBuilder.AddColumn<Guid>(
					name: "GlobalId",
					table: "Tags",
					type: "uuid",
					nullable: false,
					defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

			migrationBuilder.AlterColumn<int>(
					name: "SourceId",
					table: "Tags",
					type: "integer",
					nullable: false,
					oldClrType: typeof(int),
					oldType: "integer",
					oldNullable: true);

			migrationBuilder.AlterColumn<int>(
					name: "ParentId",
					table: "Blocks",
					type: "integer",
					nullable: true,
					oldClrType: typeof(int),
					oldType: "integer");

			migrationBuilder.AlterColumn<string>(
					name: "Name",
					table: "Sources",
					type: "text",
					nullable: false,
					defaultValue: "",
					oldClrType: typeof(string),
					oldType: "text",
					oldNullable: true);

			migrationBuilder.AddColumn<string>(
					name: "Description",
					table: "Sources",
					type: "text",
					nullable: true);

			migrationBuilder.AddColumn<DateTime>(
					name: "LastUpdate",
					table: "Settings",
					type: "timestamp with time zone",
					nullable: false,
					defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

			migrationBuilder.AddColumn<Guid>(
					name: "GlobalId",
					table: "Blocks",
					type: "uuid",
					nullable: false,
					defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

			migrationBuilder.CreateTable(
					name: "BlockProperties",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
							.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						BlockId = table.Column<int>(type: "integer", nullable: false),
						Name = table.Column<string>(type: "text", nullable: false, defaultValue: ""),
						Type = table.Column<int>(type: "integer", nullable: false),
						Value = table.Column<string>(type: "text", nullable: false),
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_BlockProperties", x => x.Id);
						table.ForeignKey(
							name: "FK_BlockProperties_Blocks_BlockId",
							column: x => x.BlockId,
							principalTable: "Blocks",
							principalColumn: "Id",
							onDelete: ReferentialAction.Cascade);
					});

			migrationBuilder.CreateTable(
					name: "TagHistoryChunks",
					columns: table => new
					{
						Table = table.Column<string>(type: "text", nullable: false),
						Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
					},
					constraints: table =>
					{
					});

			// изменение индексов

			migrationBuilder.CreateIndex(
					name: "IX_Tags_SourceId",
					table: "Tags",
					column: "SourceId");

			migrationBuilder.CreateIndex(
					name: "IX_BlockProperties_BlockId",
					table: "BlockProperties",
					column: "BlockId");

			migrationBuilder.CreateIndex(
					name: "IX_BlockTags_TagId",
					table: "BlockTags",
					column: "TagId");

			migrationBuilder.CreateIndex(
					name: "IX_TagInputs_TagId",
					table: "TagInputs",
					column: "TagId");

			// изменение ключей

			migrationBuilder.AddPrimaryKey(
					name: "PK_Users",
					table: "Users",
					column: "Name");

			migrationBuilder.AddPrimaryKey(
					name: "PK_Sources",
					table: "Sources",
					column: "Id");

			migrationBuilder.AddForeignKey(
					name: "FK_BlockTags_Blocks_BlockId",
					table: "BlockTags",
					column: "BlockId",
					principalTable: "Blocks",
					principalColumn: "Id");

			migrationBuilder.AddForeignKey(
					name: "FK_BlockTags_Tags_TagId",
					table: "BlockTags",
					column: "TagId",
					principalTable: "Tags",
					principalColumn: "Id");

			migrationBuilder.AddForeignKey(
					name: "FK_TagInputs_Tags_TagId",
					table: "TagInputs",
					column: "TagId",
					principalTable: "Tags",
					principalColumn: "Id",
					onDelete: ReferentialAction.Cascade);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			// не будет использоваться никогда
			// ахахах будет
			// ребята не стоит вскрывать эту тему
			// вы молодые, шутливые, вам все легко
			// это не то
			// это не sql запрос и даже не архивы контейнеров
			// сюда лучше не лезть
			// серьезно, любой из вас будет жалеть
			// лучше закройте тему и забудьте что тут писалось
			// я вполне понимаю что данным сообщением вызову дополнительный интерес, но хочу сразу предостеречь пытливых - стоп
			// остальные просто не поднимут
		}
	}
}
