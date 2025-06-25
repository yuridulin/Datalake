using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class EndSwitchToTimescale : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS timescaledb;");

			migrationBuilder.Sql(@"SELECT create_hypertable('""TagsHistory""', by_range('Date', INTERVAL '1 day'));");

			migrationBuilder.Sql(@"
				DO $$
				DECLARE
						tbl text;
				BEGIN
						FOR tbl IN
								SELECT quote_ident(tablename)
								FROM pg_tables
								WHERE tablename ~ '^TagsHistory_\d{4}_\d{2}_\d{2}$'
						LOOP
								EXECUTE format(
										'INSERT INTO ""TagsHistory"" (""TagId"", ""Date"", ""Text"", ""Number"", ""Quality"")
										 SELECT ""TagId"", ""Date"", ""Text"", ""Number"", ""Quality""
										 FROM %s',
										tbl
								);
						END LOOP;
				END;
				$$;");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
