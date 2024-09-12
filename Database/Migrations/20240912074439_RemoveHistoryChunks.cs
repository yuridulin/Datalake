using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class RemoveHistoryChunks : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
					name: "TagHistoryChunks",
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
		}
	}
}
