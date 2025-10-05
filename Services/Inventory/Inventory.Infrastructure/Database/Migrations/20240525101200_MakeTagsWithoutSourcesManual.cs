using Datalake.Contracts.Public.Enums;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Inventory.Infrastructure.Migrations;

/// <inheritdoc />
public partial class MakeTagsWithoutSourcesManual : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql($@"UPDATE ""Tags"" SET ""SourceId"" = {(int)SourceType.Manual} WHERE ""SourceId"" NOT IN (SELECT DISTINCT ""Id"" FROM ""Sources"");");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
	}
}
