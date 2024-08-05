using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class ChangeToDateOnlyInChunks : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<DateOnly>(
					name: "Date",
					table: "TagHistoryChunks",
					type: "date",
					nullable: false,
					oldClrType: typeof(DateTime),
					oldType: "timestamp with time zone");

			migrationBuilder.CreateTable(
					name: "TagsLive",
					columns: table => new
					{
						TagId = table.Column<int>(type: "integer", nullable: false),
						Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
						Text = table.Column<string>(type: "text", nullable: true),
						Number = table.Column<float>(type: "real", nullable: true),
						Quality = table.Column<int>(type: "integer", nullable: false),
						Using = table.Column<int>(type: "integer", nullable: false)
					},
					constraints: table =>
					{
					});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
					name: "TagsLive");

			migrationBuilder.AlterColumn<DateTime>(
					name: "Date",
					table: "TagHistoryChunks",
					type: "timestamp with time zone",
					nullable: false,
					oldClrType: typeof(DateOnly),
					oldType: "date");
		}
	}
}
