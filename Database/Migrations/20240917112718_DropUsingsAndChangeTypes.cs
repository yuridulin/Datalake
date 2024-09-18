using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class DropUsingsAndChangeTypes : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
				DO $$
				DECLARE
						r RECORD;
				BEGIN
						FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public' AND tablename LIKE 'TagsHistory_%') LOOP
								-- Удаление строк, где Using не равно 1
								EXECUTE 'DELETE FROM ' || quote_ident(r.tablename) || ' WHERE ""Using"" <> 1';
        
								-- Удаление столбца Using
								EXECUTE 'ALTER TABLE ' || quote_ident(r.tablename) || ' DROP COLUMN ""Using""';
						END LOOP;
				END $$;");

			// вот такое - из-за того, что движок не может в оптимизацию в ходе цикла и съедает все дисковое пространство
			// один скрипт тоже ест место, но меньше, по ощущениям вдвое
			migrationBuilder.Sql(@"
				DO $$
				DECLARE
						superscript text;
				BEGIN
						SELECT string_agg(script, '; ') INTO superscript
						FROM (
								SELECT concat('ALTER TABLE ""', p.tablename, '"" ALTER COLUMN ""Date"" TYPE timestamptz USING ""Date""::timestamptz') AS script
								FROM pg_tables p WHERE p.schemaname = 'public' AND p.tablename LIKE 'TagsHistory_%'
						) scripts;

						EXECUTE superscript;
				END $$;");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
