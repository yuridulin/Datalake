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
			// вот такое - из-за того, что движок не может в оптимизацию в ходе цикла и съедает все дисковое пространство
			for (int year = 2023; year <= 2026; year++)
			{
				for (int month = 1; month <= 12; month++)
				{
					for (int day = 1; day <= 31; day++)
					{
						migrationBuilder.Sql($@"
							DO $$
							BEGIN
									IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'TagsHistory_{year}_{month:D2}_{day:D2}') THEN
											-- Проверка типа столбца Date
											IF EXISTS (SELECT 1 FROM information_schema.columns 
																	WHERE table_name = 'TagsHistory_{year}_{month:D2}_{day:D2}' 
																	AND column_name = 'Date' 
																	AND data_type = 'timestamp without time zone') THEN
													ALTER TABLE ""TagsHistory_{year}_{month:D2}_{day:D2}""
													ALTER COLUMN ""Date"" TYPE timestamptz USING ""Date""::timestamptz;
											END IF;

											-- Удаление записей, где Using не равен 1
											IF EXISTS (SELECT 1 FROM information_schema.columns 
																	WHERE table_name = 'TagsHistory_{year}_{month:D2}_{day:D2}' 
																	AND column_name = 'Using') THEN
													DELETE FROM ""TagsHistory_{year}_{month:D2}_{day:D2}"" WHERE ""Using"" <> 1;
											END IF;

											-- Удаление столбца Using
											IF EXISTS (SELECT 1 FROM information_schema.columns 
																	WHERE table_name = 'TagsHistory_{year}_{month:D2}_{day:D2}' 
																	AND column_name = 'Using') THEN
													ALTER TABLE ""TagsHistory_{year}_{month:D2}_{day:D2}"" DROP COLUMN ""Using"";
											END IF;
									END IF;
							END $$;", true);
					}
				}
			}


			// проблема старого подхода к ParentId
			migrationBuilder.Sql(@"UPDATE ""Blocks"" SET ""ParentId"" = NULL WHERE ""ParentId"" = 0;");

			// откуда это, я хз
			migrationBuilder.Sql(@"ALTER TABLE ""BlockTags"" DROP COLUMN ""Type"";");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			// Так далеко мы еще не заходили...
		}
	}
}
