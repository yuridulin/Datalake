using DatalakeDatabase.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class Tag
{
	const string TableName = "Tags";

	// поля в БД

	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Column]
	public required Guid GlobalId { get; set; }

	[Column]
	public required string Name { get; set; }

	[Column]
	public string? Description { get; set; }

	[Column]
	public required TagType Type { get; set; }

	[Column, NotNull]
	public required DateTime Created { get; set; }

	// специфичные для входящих

	/// <summary>
	/// Интервал опроса значения в секундах
	/// </summary>
	[Column]
	public required short Interval { get; set; }

	[Column]
	public required int SourceId { get; set; }

	[Column]
	public string? SourceItem { get; set; } = string.Empty;

	// специфичные для числовых

	[Column]
	public required bool IsScaling { get; set; } = false;

	[Column]
	public float MinEu { get; set; } = float.MinValue;

	[Column]
	public float MaxEu { get; set; } = float.MaxValue;

	[Column]
	public float MinRaw { get; set; } = float.MinValue;

	[Column]
	public float MaxRaw { get; set; } = float.MaxValue;

	// специфичные для вычисляемых

	[Column]
	public string? Formula { get; set; }

	// связи

	[ForeignKey(nameof(SourceId))]
	[DeleteBehavior(DeleteBehavior.SetNull)]
	public Source? Source { get; set; } = null!;

	[NotMapped]
	public ICollection<TagInput> RelationsToInputTags { get; set; } = [];

	public ICollection<BlockTag> RelationsToBlocks { get; set; } = [];

	public ICollection<Block> Blocks { get; set; } = [];
}
