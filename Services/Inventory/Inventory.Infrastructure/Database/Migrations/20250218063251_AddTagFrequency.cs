using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTagFrequency : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<int>(
				name: "Frequency",
				schema: "public",
				table: "Tags",
				type: "integer",
				nullable: false,
				defaultValue: 0);

		migrationBuilder.Sql(@"
				UPDATE ""public"".""Tags""
				SET ""Frequency"" = CASE
					WHEN ""Interval"" =  0    THEN 0 -- NotSet
					WHEN ""Interval"" <= 60   THEN 1 -- ByMinute
					WHEN ""Interval"" <= 3600 THEN 2 -- ByHour
					ELSE                           3 -- ByDay
				END");

		migrationBuilder.DropColumn(
				name: "Interval",
				schema: "public",
				table: "Tags");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddColumn<short>(
				name: "Interval",
				schema: "public",
				table: "Tags",
				type: "smallint",
				nullable: false,
				defaultValue: (short)0);

		migrationBuilder.Sql(@"
				UPDATE ""public"".""Tags""
				SET ""Interval"" = CASE ""Frequency""
					WHEN 0 THEN 0     -- NotSet
					WHEN 1 THEN 60    -- ByMinute
					WHEN 2 THEN 3600  -- ByHour
					WHEN 3 THEN 86400 -- ByDay
					ELSE 0
				END");

		migrationBuilder.DropColumn(
				name: "Frequency",
				schema: "public",
				table: "Tags");
	}
}
