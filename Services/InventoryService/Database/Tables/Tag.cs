using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;

namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице тегов
/// </summary>
public record class Tag
{
	private Tag() { }

	#region поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	public required Guid GlobalGuid { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Тип значения
	/// </summary>
	public required TagType Type { get; set; }

	/// <summary>
	/// Частота записи значения
	/// </summary>
	public required TagResolution Resolution { get; set; }

	/// <summary>
	/// Дата создания
	/// </summary>
	public required DateTime Created { get; set; }

	/// <summary>
	/// Тег отмечен как удаленный
	/// </summary>
	public bool IsDeleted { get; set; } = false;

	#endregion

	#region специфичные для входящих

	/// <summary>
	/// Идентификатор источника
	/// </summary>
	public required int SourceId { get; set; }

	/// <summary>
	/// Адрес внутри источника
	/// </summary>
	public string? SourceItem { get; set; } = string.Empty;

	#endregion

	#region специфичные для числовых

	/// <summary>
	/// Используется ли преобразование по шкале
	/// </summary>
	public required bool IsScaling { get; set; } = false;

	/// <summary>
	/// Минимальное возможное значение по новой шкале
	/// </summary>
	public float MinEu { get; set; } = float.MinValue;

	/// <summary>
	/// Максимальное возможное значение по новой шкале
	/// </summary>
	public float MaxEu { get; set; } = float.MaxValue;

	/// <summary>
	/// Минимальное возможное значение по старой шкале
	/// </summary>
	public float MinRaw { get; set; } = float.MinValue;

	/// <summary>
	/// Максимальное возможное значение по старой шкале
	/// </summary>
	public float MaxRaw { get; set; } = float.MaxValue;

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
	public TagCalculation? Calculation { get; set; }

	/// <summary>
	/// Используемая формула
	/// </summary>
	public string? Formula { get; set; }

	/// <summary>
	/// Пороговые значения, по которым выбирается итоговое значение
	/// </summary>
	public List<TagThresholdInfo>? Thresholds { get; set; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета значения по таблице пороговых значений
	/// </summary>
	public int? ThresholdSourceTagId { get; set; }

	/// <summary>
	/// Идентификатор блока с тегом, который будет источником данных для расчета значения по таблице пороговых значений
	/// </summary>
	public int? ThresholdSourceTagBlockId { get; set; }

	#endregion

	#region специфичные для агрегированных

	/// <summary>
	/// Тип агрегации
	/// </summary>
	public TagAggregation? Aggregation { get; set; }

	/// <summary>
	/// Временное окно для расчета агрегированного значения
	/// </summary>
	public AggregationPeriod? AggregationPeriod { get; set; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета агрегированного значения
	/// </summary>
	public int? SourceTagId { get; set; }

	/// <summary>
	/// Идентификатор блока с тегом, который будет источником данных для расчета агрегированного значения
	/// </summary>
	public int? SourceTagBlockId { get; set; }

	#endregion

	#region связи

	/// <summary>
	/// Источник
	/// </summary>
	public Source? Source { get; set; } = null!;

	/// <summary>
	/// Тег-источник данных для агрегирования
	/// </summary>
	public Tag? SourceTag { get; set; }

	/// <summary>
	/// Тег-источник данных для вычисления по таблице пороговых значений
	/// </summary>
	public Tag? ThresholdSourceTag { get; set; }

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

	#endregion
}
