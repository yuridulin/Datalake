using Datalake.Domain.Enums;
using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;
using Datalake.Domain.ValueObjects;

namespace Datalake.Domain.Entities;

/// <summary>
/// Тег
/// </summary>
public record class Tag : IWithIdentityKey, IWithGuidKey, ISoftDeletable
{
	#region Конструкторы

	private Tag() { }

	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="type">Тип данных</param>
	/// <param name="sourceType">Тип источника данных</param>
	/// <param name="sourceId">Идентификатор источника данных</param>
	/// <param name="sourceItem">Путь к данным источника</param>
	public static Tag Create(TagType type, SourceType sourceType, int? sourceId, string? sourceItem)
	{
		var tag = new Tag
		{
			GlobalGuid = Guid.NewGuid(),
			Type = type,
			SourceItem = sourceItem,
		};

		tag.UpdateSource(sourceType, sourceId);
		return tag;
	}

	#endregion Конструкторы

	#region Методы

	/// <inheritdoc/>
	public void MarkAsDeleted()
	{
		if (IsDeleted)
			throw new DomainException("Тег уже удален");

		IsDeleted = true;
	}

	/// <summary>
	/// Установка имени по умолчанию
	/// </summary>
	/// <param name="sourceType">Тип источника</param>
	/// <param name="sourceName">Имя источника данных</param>
	public void SetGenericName(SourceType sourceType, string? sourceName)
	{
		Name = sourceType switch
		{
			SourceType.Manual => $"Мануальный тег #{Id}",
			SourceType.Calculated => $"Вычисляемый тег #{Id}",
			SourceType.Aggregated => $"Агрегатный тег #{Id}",
			SourceType.Thresholds => $"Пороговый тег #{Id}",
			SourceType.Inopc => string.IsNullOrWhiteSpace(sourceName) ? $"{SourceItem}" : $"{sourceName}.{SourceItem}",
			_ => $"Тег #{Id}"
		};
	}

	/// <summary>
	/// Изменение настроек
	/// </summary>
	/// <param name="name">Имя</param>
	/// <param name="description">Описание</param>
	/// <param name="type">Тип данных</param>
	/// <param name="resolution">Частота</param>
	/// <param name="sourceId">Идентификатор источника данных</param>
	/// <param name="sourceType">Тип источника данных</param>
	/// <param name="sourceItem">Путь к данным в источнике</param>
	/// <param name="isScaling">Используется ли шкала</param>
	/// <param name="minEu">Минимум инженерной шкалы</param>
	/// <param name="maxEu">Максимум инженерной шкалы</param>
	/// <param name="minRaw">Минимум шкалы реальных значений</param>
	/// <param name="maxRaw">Максимум шкалы реальных значений</param>
	/// <param name="formula">Формула расчета</param>
	/// <param name="aggregation">Тип агрегирования</param>
	/// <param name="aggregationPeriod">Период агрегирования</param>
	/// <param name="aggTagId">Идентификатор тега-источника агрегирования</param>
	/// <param name="aggBlockId">Иденификатор блока-источника агрегирования</param>
	/// <param name="thresholdTagId">Идентификатор тега-источника порогов</param>
	/// <param name="thresholdBlockId">Идентификатор блока-источника порогов</param>
	/// <exception cref="DomainException">Ошибки</exception>
	public void Update(
		string? name, string? description, TagType type, TagResolution resolution, int? sourceId, SourceType sourceType,
		string? sourceItem,
		bool? isScaling, float? minEu, float? maxEu, float? minRaw, float? maxRaw,
		string? formula,
		TagAggregation? aggregation, TagResolution? aggregationPeriod, int? aggTagId, int? aggBlockId,
		int? thresholdTagId, int? thresholdBlockId)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new DomainException("Название тега является обязательным");

		Name = name;
		Description = description;

		Type = type;
		Resolution = resolution;
		UpdateSource(sourceType, sourceId);

		// мы не очищаем старые настройки
		// они не будут мешать, но будут полезны при возможном обратном изменении

		if (Type == TagType.Number)
		{
			UpdateNumericConfig(isScaling, minEu, maxEu, minRaw, maxRaw);
		}

		if (sourceType == SourceType.Inopc)
		{
			UpdateInopcConfig(sourceItem);
		}
		else if (sourceType == SourceType.Calculated)
		{
			UpdateCalculationConfig(formula);
		}
		else if (sourceType == SourceType.Aggregated)
		{
			UpdateAggregationConfig(aggregation, aggregationPeriod, aggTagId, aggBlockId);
		}
		else if (sourceType == SourceType.Thresholds)
		{
			UpdateThresholdsConfig(thresholdTagId, thresholdBlockId);
		}
	}

	private void UpdateSource(SourceType sourceType, int? sourceId)
	{
		SourceId = sourceType switch
		{
			SourceType.Unset => throw new DomainException("Тип источника является обязательным для тега"),
			SourceType.System => throw new DomainException("Создавать системные теги запрещено"),
			SourceType.Aggregated => (int)SourceType.Aggregated,
			SourceType.Calculated => (int)SourceType.Calculated,
			SourceType.Manual => (int)SourceType.Manual,
			SourceType.Thresholds => (int)SourceType.Thresholds,
			SourceType.Inopc => sourceId.HasValue && sourceId.Value > 0 ? sourceId.Value : throw new DomainException("Идентификатор источника Inopc не передан"),
			SourceType.Datalake => sourceId.HasValue && sourceId.Value > 0 ? sourceId.Value : throw new DomainException("Идентификатор источника Datalake не передан"),
			_ => throw new NotImplementedException("Неизвестный тип источника данных"),
		};
	}

	private void UpdateThresholdsConfig(int? thresholdTagId, int? thresholdBlockId)
	{
		if (!thresholdTagId.HasValue)
			throw new DomainException("Для порогового тега обязан быть указан тег-источник");

		ThresholdSourceTagId = thresholdTagId;
		ThresholdSourceTagBlockId = thresholdBlockId;
	}

	private void UpdateAggregationConfig(TagAggregation? aggregation, TagResolution? aggregationPeriod, int? aggTagId, int? aggBlockId)
	{
		if (!aggregation.HasValue)
			throw new DomainException("Тип агрегирования является обязательным для агрегатного тега");

		if (!aggregationPeriod.HasValue)
			throw new DomainException("Период агрегирования является обязательным для агрегатного тега");

		if (!aggTagId.HasValue)
			throw new DomainException("Для агрегатного тега обязан быть указан тег-источник");

		Aggregation = aggregation.Value;
		AggregationPeriod = aggregationPeriod.Value;
		SourceTagId = aggTagId;
		SourceTagBlockId = aggBlockId;
	}

	private void UpdateCalculationConfig(string? formula)
	{
		if (string.IsNullOrWhiteSpace(formula))
			throw new DomainException("Для вычисляемого тега формула является обязательной");

		Formula = formula;
	}

	private void UpdateInopcConfig(string? sourceItem)
	{
		if (string.IsNullOrWhiteSpace(sourceItem))
			throw new DomainException("Для тегов Inopc путь к значению является обязательным");

		SourceItem = sourceItem;
	}

	private void UpdateNumericConfig(bool? isScaling, float? minEu, float? maxEu, float? minRaw, float? maxRaw)
	{
		if (isScaling == null)
			throw new DomainException("В числовых настройках не передана настройка использования шкалы");

		IsScaling = isScaling.Value;

		minEu ??= float.MinValue;
		maxEu ??= float.MaxValue;
		minRaw ??= float.MinValue;
		maxRaw ??= float.MaxValue;

		if (minEu.Value >= maxEu.Value)
			throw new DomainException("В шкале инженерных значений начальное значение должно быть меньше конечного");

		if (minRaw.Value >= maxRaw.Value)
			throw new DomainException("В шкале исходных значений начальное значение должно быть меньше конечного");

		MinEu = minEu.Value;
		MaxEu = maxEu.Value;
		MinRaw = minRaw.Value;
		MaxRaw = maxRaw.Value;
	}

	#endregion Методы

	#region Свойства

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	public Guid GlobalGuid { get; private set; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	public Guid Guid => GlobalGuid;

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; private set; } = string.Empty;

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

	#endregion Свойства

	#region Свойства, специфичные для входящих

	/// <summary>
	/// Идентификатор источника
	/// </summary>
	public int SourceId { get; private set; }

	/// <summary>
	/// Адрес внутри источника
	/// </summary>
	public string? SourceItem { get; private set; } = string.Empty;

	#endregion Свойства, специфичные для входящих

	#region Свойства, специфичные для числовых

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

	#endregion Свойства, специфичные для числовых

	#region Свойства, специфичные для вычисляемых

	/// <summary>
	/// Используемая формула
	/// </summary>
	public string? Formula { get; private set; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета значения по таблице пороговых значений
	/// </summary>
	public int? ThresholdSourceTagId { get; private set; }

	/// <summary>
	/// Идентификатор блока с тегом, который будет источником данных для расчета значения по таблице пороговых значений
	/// </summary>
	public int? ThresholdSourceTagBlockId { get; private set; }

	#endregion Свойства, специфичные для вычисляемых

	#region Свойства, специфичные для агрегированных

	/// <summary>
	/// Тип агрегации
	/// </summary>
	public TagAggregation? Aggregation { get; private set; }

	/// <summary>
	/// Временное окно для расчета агрегированного значения
	/// </summary>
	public TagResolution? AggregationPeriod { get; private set; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета агрегированного значения
	/// </summary>
	public int? SourceTagId { get; private set; }

	/// <summary>
	/// Идентификатор блока с тегом, который будет источником данных для расчета агрегированного значения
	/// </summary>
	public int? SourceTagBlockId { get; private set; }

	#endregion Свойства, специфичные для агрегированных

	#region Связи

	/// <summary>
	/// Источник
	/// </summary>
	public Source? Source { get; set; } = null!;

	/// <summary>
	/// Тег-источник данных для агрегирования
	/// </summary>
	public Tag? AggregationSourceTag { get; set; }

	/// <summary>
	/// Тег-источник данных для вычисления по таблице пороговых значений
	/// </summary>
	public Tag? ThresholdsSourceTag { get; set; }

	/// <summary>
	/// Входные теги для формул
	/// </summary>
	public ICollection<TagInput> Inputs { get; set; } = [];

	/// <summary>
	/// Пороговые значения
	/// </summary>
	public ICollection<TagThreshold> Thresholds { get; set; } = [];

	/// <summary>
	/// Список прямых правил доступа на этот тег
	/// </summary>
	public ICollection<AccessRule> AccessRules { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита по этому тегу
	/// </summary>
	public ICollection<AuditLog> AuditLogs { get; set; } = [];

	/// <summary>
	/// Список связей с блоками, в которых состоит тег
	/// </summary>
	public ICollection<BlockTag> RelationsToBlocks { get; set; } = [];

	/// <summary>
	/// Список тегов, подписанных на этот тег для агрегации
	/// </summary>
	public ICollection<Tag>? AggregateTagsUsingThisTag { get; set; }

	/// <summary>
	/// Список тегов, подписанных на этот тег для расчета по порогам
	/// </summary>
	public ICollection<Tag>? ThresholdsTagsUsingThisTag { get; set; }

	/// <summary>
	/// Связи, в которых этот тег выступает входным
	/// </summary>
	public ICollection<TagInput> InputsUsingThisTag { get; set; } = [];

	/// <summary>
	/// Рассчитаные для этого тега указания фактического доступа
	/// </summary>
	public ICollection<CalculatedAccessRule> CalculatedAccessRules { get; set; } = [];

	#endregion Связи
}
