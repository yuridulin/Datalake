using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddSessions : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.CreateTable(
				name: "UserSessions",
				schema: "public",
				columns: table => new
				{
					UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
					Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
					Token = table.Column<string>(type: "text", nullable: false),
					Type = table.Column<int>(type: "integer", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserSessions", x => x.UserGuid);
					table.ForeignKey(
						name: "FK_UserSessions_Users_UserGuid",
						column: x => x.UserGuid,
						principalSchema: "public",
						principalTable: "Users",
						principalColumn: "Guid",
						onDelete: ReferentialAction.Cascade);
				});
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropTable(
				name: "UserSessions",
				schema: "public");
	}
}
