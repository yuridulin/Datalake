using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Datalake.Gateway.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class Initial : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
					name: "gateway");

			migrationBuilder.RenameTable(
					name: "UserSessions",
					schema: "public",
					newName: "UserSessions",
					newSchema: "gateway");

			/*migrationBuilder.CreateTable(
					name: "UserSessions",
					schema: "gateway",
					columns: table => new
					{
						Id = table.Column<int>(type: "integer", nullable: false)
									.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
						UserGuid = table.Column<Guid>(type: "uuid", nullable: false),
						Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
						ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
						Token = table.Column<string>(type: "text", nullable: false),
						Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_UserSessions", x => x.Id);
					});*/

			migrationBuilder.CreateIndex(
					name: "IX_UserSessions_UserGuid",
					schema: "gateway",
					table: "UserSessions",
					column: "UserGuid");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
					name: "UserSessions",
					schema: "gateway");
		}
	}
}
