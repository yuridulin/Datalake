using DatalakeApiClasses.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class BlockProperty
{
	const string TableName = "BlockProperties";

	// поля в БД

	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Column]
	public int BlockId { get; set; }

	[Column, NotNull]
	public string Name { get; set; } = string.Empty;

	[Column]
	public TagType Type { get; set; } = TagType.String;

	[Column]
	public string Value { get; set; } = string.Empty;

	// связи

	[ForeignKey(nameof(BlockId))]
	public Block? Block { get; set; }
}
