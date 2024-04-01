using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DatalakeDatabase.Migrations
{
    /// <inheritdoc />
    public partial class EnstablishRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Sources",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

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

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sources",
                table: "Sources",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BlockProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BlockId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Sources_SourceId",
                table: "Tags",
                column: "SourceId",
                principalTable: "Sources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Sources_SourceId",
                table: "Tags");

            migrationBuilder.DropTable(
                name: "BlockProperties");

            migrationBuilder.DropTable(
                name: "BlockTags");

            migrationBuilder.DropTable(
                name: "TagHistoryChunks");

            migrationBuilder.DropTable(
                name: "TagInputs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tags_SourceId",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sources",
                table: "Sources");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "LastUpdate",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "GlobalId",
                table: "Blocks");

            migrationBuilder.RenameColumn(
                name: "MinEu",
                table: "Tags",
                newName: "MinEU");

            migrationBuilder.RenameColumn(
                name: "MaxEu",
                table: "Tags",
                newName: "MaxEU");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Sources",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Settings",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "Settings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PropertiesRaw",
                table: "Blocks",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Logs",
                columns: table => new
                {
                    Category = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true),
                    Ref = table.Column<int>(type: "integer", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    User = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                });

                migrationBuilder.RenameTable(
                    newName: "Rel_Block_Tag",
                    name: "BlockTags");

                migrationBuilder.RenameTable(
                    newName: "Rel_Tag_Input",
                    name: "TagInputs");
        }
    }
}
