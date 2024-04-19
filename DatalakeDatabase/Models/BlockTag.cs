using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class BlockTag
{
	public const string TableName = "BlockTags";

	// поля в БД

	[Column]
	public int BlockId { get; set; }

	[Column]
	public int TagId { get; set; }

	[Column]
	public string? Name { get; set; } = string.Empty;

	// связи

	public Block? Block { get; set; }

	public Tag? Tag { get; set; }
}
