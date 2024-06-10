using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatalakeDatabase.Migrations
{
    /// <inheritdoc />
    public partial class RenameKeycloakToEnergoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "KeycloakGuid",
                table: "Users",
                newName: "EnergoIdGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnergoIdGuid",
                table: "Users",
                newName: "KeycloakGuid");
        }
    }
}
