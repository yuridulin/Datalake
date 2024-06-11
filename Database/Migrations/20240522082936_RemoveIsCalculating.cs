using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatalakeDatabase.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsCalculating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCalculating",
                table: "Tags");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCalculating",
                table: "Tags",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
