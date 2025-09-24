using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations;

/// <inheritdoc />
public partial class ChangeSourceType : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(@"
				UPDATE public.""Sources""
				SET ""Type"" = CASE
						WHEN ""Id"" > 0 THEN ""Type"" + 1
						ELSE ""Id""
				END;");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(@"
				UPDATE public.""Sources""
				SET ""Type"" = CASE
						WHEN ""Id"" >= 0 THEN ""Type"" - 1
						ELSE -1
				END;");
	}
}
