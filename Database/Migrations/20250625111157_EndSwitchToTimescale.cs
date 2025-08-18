using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations;

/// <inheritdoc />
public partial class EndSwitchToTimescale : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS timescaledb;", true);

		migrationBuilder.Sql(@"SELECT create_hypertable(
				'public.""TagsHistory""',
				'Date',
				partitioning_column => 'TagId', -- пространство: колонка TagId
				number_partitions    => 8, -- сколько «корзин» по TagId
				chunk_time_interval  => INTERVAL '7 days' -- временной интервал
			);", true);

		for (int year = 2024; year <= 2026; year++)
		{
			for (int month = 1; month <= 12; month++)
			{
				for (int day = 1; day <= 31; day++)
				{
					string tableDate = $"{year}_{month:D2}_{day:D2}";
					migrationBuilder.Sql($@"
						DO $$
						BEGIN
							IF EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'TagsHistory_{tableDate}') THEN
								INSERT INTO ""TagsHistory"" (""TagId"", ""Date"", ""Text"", ""Number"", ""Quality"")
								SELECT ""TagId"", ""Date"", ""Text"", ""Number"", ""Quality""
								FROM ""TagsHistory_{tableDate}"";
								DROP TABLE ""TagsHistory_{tableDate}"";
							END IF;
						END $$;", true);
				}
			}
		}
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{

	}
}
