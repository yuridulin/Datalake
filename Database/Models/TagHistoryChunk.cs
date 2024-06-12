using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using PrimaryKeyAttribute = LinqToDB.Mapping.PrimaryKeyAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class TagHistoryChunk
{
	const string TableName = "TagHistoryChunks";

	// поля в БД

	[Column, Key, PrimaryKey]
	public DateOnly Date { get; set; } = DateOnly.MinValue;

	[Column]
	public required string Table { get; set; } = string.Empty;
}
