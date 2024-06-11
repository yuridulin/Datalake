using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatalakeDatabase.Migrations
{
    /// <inheritdoc />
    public partial class ChangeLogRefId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RefId",
                table: "Logs",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.Sql(
                "UPDATE \"Tags\" " +
                "SET \"GlobalGuid\" = gen_random_uuid() " +
                "WHERE \"GlobalGuid\" = '00000000-0000-0000-0000-000000000000';");

            migrationBuilder.Sql(
                "UPDATE \"Tags\" " +
                "SET \"Created\" = now() " +
                "WHERE \"Created\" = '-infinity';");

            migrationBuilder.Sql(
                "UPDATE \"Blocks\" " +
                "SET \"GlobalId\" = gen_random_uuid() " +
                "WHERE \"GlobalId\" = '00000000-0000-0000-0000-000000000000';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RefId",
                table: "Logs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
