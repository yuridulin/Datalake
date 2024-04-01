using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DatalakeDatabase.Migrations
{
    /// <inheritdoc />
    public partial class SwitchToEF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PropertiesRaw",
                table: "Blocks");

            migrationBuilder.Sql(@"DO $$
              DECLARE
                  _tbl text;
                  _column_to_drop text = 'Type';
              BEGIN
                  FOR _tbl IN (
                      SELECT table_name
                      FROM information_schema.tables
                      WHERE lower(table_name) LIKE 'tagshistory_%'
                  )
                  LOOP
                      EXECUTE format('ALTER TABLE %I DROP COLUMN IF EXISTS %I', _tbl, _column_to_drop);
                  END LOOP;
              END $$;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PropertiesRaw",
                table: "Blocks",
                type: "text",
                nullable: true);
        }
    }
}
