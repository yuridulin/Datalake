using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Datalake.Database.Migrations;

/// <inheritdoc />
public partial class AddLogTable : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
				name: "Logs",
				columns: table => new
				{
					Id = table.Column<long>(type: "bigint", nullable: false)
								.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
					Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					Category = table.Column<int>(type: "integer", nullable: false),
					RefId = table.Column<int>(type: "integer", nullable: true),
					Type = table.Column<int>(type: "integer", nullable: false),
					Text = table.Column<string>(type: "text", nullable: false),
					Details = table.Column<string>(type: "text", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Logs", x => x.Id);
				});
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
				name: "Logs");
	}
}
