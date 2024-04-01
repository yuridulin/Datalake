using DatalakeDatabase.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public partial class Tag
{
	const string TableName = "Tags";

	// поля в БД

	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Column]
	public Guid GlobalId { get; set; } = Guid.Empty;

	[Column]
	public string Name { get; set; } = null!;

	[Column]
	public string? Description { get; set; }

	[Column]
	public TagType Type { get; set; }

	[Column, NotNull]
	public DateTime Created { get; set; }

	// специфичные для входящих

	[Column]
	public short Interval { get; set; }

	[Column]
	public int? SourceId { get; set; }

	[Column]
	public string? SourceItem { get; set; } = string.Empty;

	// специфичные для числовых

	[Column]
	public bool IsScaling { get; set; } = false;

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
	public bool IsCalculating { get; set; }

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
