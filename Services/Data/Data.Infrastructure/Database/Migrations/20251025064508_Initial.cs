using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Data.Infrastructure.Database.Migrations
{
	/// <inheritdoc />
	public partial class Initial : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.EnsureSchema(
					name: "data");

			migrationBuilder.CreateTable(
					name: "TagsValues",
					schema: "data",
					columns: table => new
					{
						TagId = table.Column<int>(type: "integer", nullable: false),
						Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
						Text = table.Column<string>(type: "text", nullable: true),
						Number = table.Column<float>(type: "real", nullable: true),
						Boolean = table.Column<bool>(type: "boolean", nullable: true),
						Quality = table.Column<byte>(type: "smallint", nullable: false)
					},
					constraints: table =>
					{
						table.PrimaryKey("PK_TagsValues", x => new { x.TagId, x.Date });
					});

			migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS timescaledb SCHEMA data;", true);

			migrationBuilder.Sql(@"SELECT create_hypertable(
				'data.""TagsValues""',
				'Date',
				partitioning_column => 'TagId', -- пространство: колонка TagId
				number_partitions    => 8, -- сколько «корзин» по TagId
				chunk_time_interval  => INTERVAL '7 days' -- временной интервал
			);", true);
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
					name: "TagsValues",
					schema: "data");
		}
	}
}
