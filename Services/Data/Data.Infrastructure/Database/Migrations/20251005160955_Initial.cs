using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Data.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class Initial : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
					name: "data");

			migrationBuilder.RenameTable(
					name: "TagsHistory",
					schema: "public",
					newName: "TagsHistory",
					newSchema: "data");

			/*migrationBuilder.CreateTable(
					name: "TagsHistory",
					schema: "data",
					columns: table => new
					{
							TagId = table.Column<int>(type: "integer", nullable: false),
							Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
							Text = table.Column<string>(type: "text", nullable: true),
							Number = table.Column<float>(type: "real", nullable: true),
							Boolean = table.Column<bool>(type: "boolean", nullable: true),
							Quality = table.Column<byte>(type: "smallint", nullable: false)
					},
					constraints: table =>
					{
					});

			migrationBuilder.CreateIndex(
					name: "TagsHistory_TagId_Date_idx",
					schema: "data",
					table: "TagsHistory",
					columns: new[] { "TagId", "Date" },
					unique: true,
					descending: new[] { false, true });*/
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			/*migrationBuilder.DropTable(
					name: "TagsHistory",
					schema: "data");*/
		}
	}
}
