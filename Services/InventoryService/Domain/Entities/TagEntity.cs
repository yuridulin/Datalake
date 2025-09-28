using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;

namespace Datalake.InventoryService.Domain.Entities;

/// <summary>
/// Запись в таблице тегов
/// </summary>
public record class TagEntity
{
	private TagEntity() { }

	#region поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	public Guid GlobalGuid { get; private set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; private set; }

	/// <summary>
	/// Описание
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// Тип значения
	/// </summary>
	public TagType Type { get; private set; }

	/// <summary>
	/// Частота записи значения
	/// </summary>
	public TagResolution Resolution { get; private set; }

	/// <summary>
	/// Дата создания
	/// </summary>
	public DateTime Created { get; private set; }

	/// <summary>
	/// Тег отмечен как удаленный
	/// </summary>
	public bool IsDeleted { get; private set; } = false;

	#endregion

	#region специфичные для входящих

	/// <summary>
	/// Идентификатор источника
	/// </summary>
	public int SourceId { get; private set; }

	/// <summary>
	/// Адрес внутри источника
	/// </summary>
	public string? SourceItem { get; private set; } = string.Empty;

	#endregion

	#region специфичные для числовых

	/// <summary>
	/// Используется ли преобразование по шкале
	/// </summary>
	public bool IsScaling { get; private set; } = false;

	/// <summary>
	/// Минимальное возможное значение по новой шкале
	/// </summary>
	public float MinEu { get; private set; } = float.MinValue;

	/// <summary>
	/// Максимальное возможное значение по новой шкале
	/// </summary>
	public float MaxEu { get; private set; } = float.MaxValue;

	/// <summary>
	/// Минимальное возможное значение по старой шкале
	/// </summary>
	public float MinRaw { get; private set; } = float.MinValue;

	/// <summary>
	/// Максимальное возможное значение по старой шкале
	/// </summary>
	public float MaxRaw { get; private set; } = float.MaxValue;

	/// <summary>
	/// Коэффициент преобразования по шкалам, вычисляется при получении
	/// </summary>
	public float GetScalingCoefficient() => IsScaling
		? ((MaxEu - MinEu) / (MaxRaw - MinRaw))
		: 1;

	#endregion

	#region специфичные для вычисляемых

	/// <summary>
	/// Используемый тип вычисления
	/// </summary>
	public TagCalculation? Calculation { get; private set; }

	/// <summary>
	/// Используемая формула
	/// </summary>
	public string? Formula { get; private set; }

	/// <summary>
	/// Пороговые значения, по которым выбирается итоговое значение
	/// </summary>
	public List<TagThresholdInfo>? Thresholds { get; private set; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета значения по таблице пороговых значений
	/// </summary>
	public int? ThresholdSourceTagId { get; private set; }

	/// <summary>
	/// Идентификатор блока с тегом, который будет источником данных для расчета значения по таблице пороговых значений
	/// </summary>
	public int? ThresholdSourceTagBlockId { get; private set; }

	#endregion

	#region специфичные для агрегированных

	/// <summary>
	/// Тип агрегации
	/// </summary>
	public TagAggregation? Aggregation { get; private set; }

	/// <summary>
	/// Временное окно для расчета агрегированного значения
	/// </summary>
	public AggregationPeriod? AggregationPeriod { get; private set; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета агрегированного значения
	/// </summary>
	public int? SourceTagId { get; private set; }

	/// <summary>
	/// Идентификатор блока с тегом, который будет источником данных для расчета агрегированного значения
	/// </summary>
	public int? SourceTagBlockId { get; private set; }

	#endregion

	#region связи

	/// <summary>
	/// Источник
	/// </summary>
	public SourceEntity? Source { get; set; } = null!;

	/// <summary>
	/// Тег-источник данных для агрегирования
	/// </summary>
	public TagEntity? SourceTag { get; set; }

	/// <summary>
	/// Тег-источник данных для вычисления по таблице пороговых значений
	/// </summary>
	public TagEntity? ThresholdSourceTag { get; set; }

	/// <summary>
	/// Входные теги
	/// </summary>
	public ICollection<TagInputEntity> Inputs { get; set; } = [];

	/// <summary>
	/// Список связей с блоками
	/// </summary>
	public ICollection<BlockTagEntity> RelationsToBlocks { get; set; } = [];

	/// <summary>
	/// Список блоков
	/// </summary>
	public ICollection<BlockEntity> Blocks { get; set; } = [];

	/// <summary>
	/// Список правил доступа, действующих на тег
	/// </summary>
	public ICollection<AccessRuleEntity> AccessRightsList { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<AuditEntity> Logs { get; set; } = null!;

	#endregion
}
