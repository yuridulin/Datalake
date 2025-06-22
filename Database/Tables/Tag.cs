using Datalake.Database.Interfaces;
using Datalake.PublicApi.Enums;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице тегов
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public record class Tag : IReadOnlyTag
{
	const string TableName = "Tags";

	/// <summary>
	/// Да это же конструктор! Пустой для LinqToDB
	/// </summary>
	public Tag() { }

	// поля в БД

	/// <inheritdoc/>
	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	/// <inheritdoc/>
	[Column]
	public required Guid GlobalGuid { get; set; }

	/// <inheritdoc/>
	[Column]
	public required string Name { get; set; }

	/// <inheritdoc/>
	[Column]
	public string? Description { get; set; }

	/// <inheritdoc/>
	[Column]
	public required TagType Type { get; set; }

	/// <inheritdoc/>
	[Column]
	public required TagFrequency Frequency { get; set; }

	/// <inheritdoc/>
	[Column, NotNull]
	public required DateTime Created { get; set; }

	/// <inheritdoc/>
	[Column, Required]
	public bool IsDeleted { get; set; }

	// специфичные для входящих

	/// <inheritdoc/>
	[Column]
	public required int SourceId { get; set; }

	/// <inheritdoc/>
	[Column]
	public string? SourceItem { get; set; }

	// специфичные для числовых

	/// <inheritdoc/>
	[Column]
	public required bool IsScaling { get; set; }

	/// <inheritdoc/>
	[Column]
	public float MinEu { get; set; }

	/// <inheritdoc/>
	[Column]
	public float MaxEu { get; set; }

	/// <inheritdoc/>
	[Column]
	public float MinRaw { get; set; }

	/// <inheritdoc/>
	[Column]
	public float MaxRaw { get; set; }

	// специфичные для вычисляемых

	/// <inheritdoc/>
	[Column]
	public string? Formula { get; set; }

	// специфичные для агрегированных

	/// <inheritdoc/>
	[Column]
	public TagAggregation? Aggregation { get; set; }

	/// <inheritdoc/>
	[Column]
	public AggregationPeriod? AggregationPeriod { get; set; }

	/// <inheritdoc/>
	[Column]
	public int? SourceTagId { get; set; }

	// связи

	/// <summary>
	/// Источник
	/// </summary>
	public Source? Source { get; set; } = null!;

	/// <summary>
	/// Тег-источник данных для агрегирования
	/// </summary>
	public Tag? SourceTag { get; set; }

	/// <summary>
	/// Входные теги
	/// </summary>
	public ICollection<TagInput> Inputs { get; set; } = [];

	/// <summary>
	/// Список связей с блоками
	/// </summary>
	public ICollection<BlockTag> RelationsToBlocks { get; set; } = [];

	/// <summary>
	/// Список блоков
	/// </summary>
	public ICollection<Block> Blocks { get; set; } = [];

	/// <summary>
	/// Список правил доступа, действующих на тег
	/// </summary>
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<Log> Logs { get; set; } = null!;
}
