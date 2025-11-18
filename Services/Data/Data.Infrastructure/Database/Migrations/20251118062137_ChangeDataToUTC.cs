using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Data.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class ChangeDataToUTC : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@"
				-- 1.1 Создаём временную таблицу со структурой как у data.""TagsValues""
				CREATE TABLE data.""TagsValues_tmp"" (
					""TagId""   int4 NOT NULL,
					""Date""    timestamptz NOT NULL,
					""Text""    text NULL,
					""Number""  float4 NULL,
					""Boolean"" bool NULL,
					""Quality"" int2 NOT NULL,
					CONSTRAINT ""PK_TagsValues_tmp"" PRIMARY KEY (""TagId"", ""Date"")
				);");

			migrationBuilder.Sql(@"
				-- 2.1 Превращаем во hypertable с пространственным партиционированием
				SELECT create_hypertable(
					'data.""TagsValues_tmp""',
					'Date',
					partitioning_column => 'TagId',
					number_partitions   => 8,
					chunk_time_interval => INTERVAL '7 days',
					if_not_exists       => true
				);");

			migrationBuilder.Sql(@"
				-- 3 Перенос данных с изменением часового пояса на UTC
				INSERT INTO data.""TagsValues_tmp"" (""TagId"",""Date"",""Text"",""Number"",""Boolean"",""Quality"")
				SELECT
					""TagId"",
					(""Date"" AT TIME ZONE 'Europe/Minsk') AT TIME ZONE 'UTC' AS ""Date"",
					""Text"",""Number"",""Boolean"",""Quality""
				FROM data.""TagsValues"";");

			migrationBuilder.Sql(@"
				-- 4.1 Переименовать старую таблицу в архив (на случай отката)
				ALTER TABLE data.""TagsValues"" RENAME TO ""TagsValues_old"";");

			migrationBuilder.Sql(@"
				-- 4.2 Переименовать новую таблицу на боевое имя
				ALTER TABLE data.""TagsValues_tmp"" RENAME TO ""TagsValues"";");

			migrationBuilder.Sql(@"
				-- 4.3 Удалить старую таблицу
				DROP TABLE ""data"".""TagsValues_old"";");

			migrationBuilder.Sql(@"
				-- 4.4 Переименовать индекс под новое имя (не обязательно, но красиво)
				ALTER INDEX ""data"".""PK_TagsValues_tmp"" RENAME TO ""PK_TagsValues"";
				ALTER INDEX ""data"".""TagsValues_tmp_Date_idx"" RENAME TO ""TagsValues_Date_idx"";");
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
		}
	}
}
