using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations
{
	/// <inheritdoc />
	public partial class AddOnConflictCheck : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			/*migrationBuilder.Sql(@"
					DELETE FROM ""TagsHistory"" th
					USING (
							SELECT MIN(ctid) AS keep_ctid, ""TagId"", ""Date""
							FROM ""TagsHistory""
							GROUP BY ""TagId"", ""Date""
							HAVING COUNT(*) > 1
					) dup
					WHERE th.""TagId"" = dup.""TagId""
						AND th.""Date"" = dup.""Date""
						AND th.ctid <> dup.keep_ctid;
					");*/

			/*migrationBuilder.Sql(@"
					-- Удаляем существующий неуникальный индекс
					DROP INDEX IF EXISTS ""TagsHistory_TagId_Date_idx"";

					-- Создаём уникальный индекс с нужным порядком сортировки
					CREATE UNIQUE INDEX ""TagsHistory_TagId_Date_idx""
					ON ""TagsHistory"" (""TagId"" ASC, ""Date"" DESC);");*/

			/*migrationBuilder.CreateIndex(
					name: "TagsHistory_TagId_Date_idx",
					schema: "public",
					table: "TagsHistory",
					columns: new[] { "TagId", "Date" },
					unique: true,
					descending: new[] { false, true });*/
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			/*migrationBuilder.Sql(@"
					-- Удаляем существующий уникальный индекс
					DROP INDEX IF EXISTS ""TagsHistory_TagId_Date_idx"";

					-- Создаём неуникальный индекс с нужным порядком сортировки
					CREATE INDEX ""TagsHistory_TagId_Date_idx""
					ON ""TagsHistory"" (""TagId"" ASC, ""Date"" DESC);");*/

			/*migrationBuilder.DropIndex(
					name: "TagsHistory_TagId_Date_idx",
					schema: "public",
					table: "TagsHistory");*/
		}
	}
}
