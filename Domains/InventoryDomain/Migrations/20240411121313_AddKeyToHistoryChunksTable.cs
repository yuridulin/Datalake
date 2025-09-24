using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Datalake.Database.Migrations;

/// <inheritdoc />
public partial class AddKeyToHistoryChunksTable : Migration
{
	/// <inheritdoc />
	protected override void Up(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.AddPrimaryKey(
				name: "PK_TagHistoryChunks",
				table: "TagHistoryChunks",
				column: "Date");
	}

	/// <inheritdoc />
	protected override void Down(MigrationBuilder migrationBuilder)
	{
		migrationBuilder.DropPrimaryKey(
				name: "PK_TagHistoryChunks",
				table: "TagHistoryChunks");
	}
}
