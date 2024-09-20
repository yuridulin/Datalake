using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
    /// <inheritdoc />
    public partial class DropSomeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TagHistoryChunks",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TagsLive",
                schema: "public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TagHistoryChunks",
                schema: "public",
                columns: table => new
                {
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Table = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TagHistoryChunks", x => x.Date);
                });

            migrationBuilder.CreateTable(
                name: "TagsLive",
                schema: "public",
                columns: table => new
                {
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Number = table.Column<float>(type: "real", nullable: true),
                    Quality = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: true),
                    Using = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                });
        }
    }
}
