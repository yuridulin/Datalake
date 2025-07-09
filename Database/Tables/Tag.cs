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
public record class Tag
{
	const string TableName = "Tags";

	/// <summary>
	/// Да это же конструктор! Пустой для LinqToDB
	/// </summary>
	public Tag() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	[Column]
	public required Guid GlobalGuid { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	[Column]
	public required string Name { get; set; }

	/// <summary>
	/// Описание
	/// </summary>
	[Column]
	public string? Description { get; set; }

	/// <summary>
	/// Тип значения
	/// </summary>
	[Column]
	public required TagType Type { get; set; }

	/// <summary>
	/// Частота записи значения
	/// </summary>
	[Column]
	public required TagFrequency Frequency { get; set; }

	/// <summary>
	/// Дата создания
	/// </summary>
	[Column, NotNull]
	public required DateTime Created { get; set; }

	/// <summary>
	/// Тег отмечен как удаленный
	/// </summary>
	[Column, Required]
	public bool IsDeleted { get; set; } = false;

	// специфичные для входящих

	/// <summary>
	/// Идентификатор источника
	/// </summary>
	[Column]
	public required int SourceId { get; set; }

	/// <summary>
	/// Адрес внутри источника
	/// </summary>
	[Column]
	public string? SourceItem { get; set; } = string.Empty;

	// специфичные для числовых

	/// <summary>
	/// Используется ли преобразование по шкале
	/// </summary>
	[Column]
	public required bool IsScaling { get; set; } = false;

	/// <summary>
	/// Минимальное возможное значение по новой шкале
	/// </summary>
	[Column]
	public float MinEu { get; set; } = float.MinValue;

	/// <summary>
	/// Максимальное возможное значение по новой шкале
	/// </summary>
	[Column]
	public float MaxEu { get; set; } = float.MaxValue;

	/// <summary>
	/// Минимальное возможное значение по старой шкале
	/// </summary>
	[Column]
	public float MinRaw { get; set; } = float.MinValue;

	/// <summary>
	/// Максимальное возможное значение по старой шкале
	/// </summary>
	[Column]
	public float MaxRaw { get; set; } = float.MaxValue;

	// специфичные для вычисляемых

	/// <summary>
	/// Используемая формула
	/// </summary>
	[Column]
	public string? Formula { get; set; }

	// специфичные для агрегированных

	/// <summary>
	/// Тип агрегации
	/// </summary>
	[Column]
	public TagAggregation? Aggregation { get; set; }

	/// <summary>
	/// Временное окно для расчета агрегированного значения
	/// </summary>
	[Column]
	public AggregationPeriod? AggregationPeriod { get; set; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета агрегированного значения
	/// </summary>
	[Column]
	public int? SourceTagId { get; set; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета агрегированного значения
	/// </summary>
	[Column]
	public int? SourceTagRelationId { get; set; }

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
