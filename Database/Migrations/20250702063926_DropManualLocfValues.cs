using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations;

/// <inheritdoc />
public partial class DropManualLocfValues : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.Sql(@"
				DELETE FROM ""TagsHistory""
				WHERE ""TagId"" NOT IN (
					SELECT ""Id"" FROM ""Tags""
				)", true);

		migrationBuilder.Sql(@"
				DELETE FROM ""TagsHistory""
				WHERE ""Quality"" = ANY(ARRAY[100,200])", true);
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{

	}
}
