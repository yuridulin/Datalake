using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Data.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddBoolean : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>(
					name: "Boolean",
					schema: "data",
					table: "TagsHistory",
					type: "boolean",
					nullable: true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
					name: "Boolean",
					schema: "data",
					table: "TagsHistory");
		}
	}
}
