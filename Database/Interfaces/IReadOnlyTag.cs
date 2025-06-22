using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;

namespace Datalake.Database.Interfaces;

/// <summary>
/// Модель тега, защищенная от записи
/// </summary>
public interface IReadOnlyTag
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	int Id { get; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	Guid GlobalGuid { get; }

	/// <summary>
	/// Название
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Описание
	/// </summary>
	string? Description { get; }

	/// <summary>
	/// Тип значения
	/// </summary>
	TagType Type { get; }

	/// <summary>
	/// Частота записи значения
	/// </summary>
	TagFrequency Frequency { get; }

	/// <summary>
	/// Дата создания
	/// </summary>
	DateTime Created { get; }

	/// <summary>
	/// Тег отмечен как удаленный
	/// </summary>
	bool IsDeleted { get; }

	/// <summary>
	/// Идентификатор источника
	/// </summary>
	int SourceId { get; }

	/// <summary>
	/// Адрес внутри источника
	/// </summary>
	string? SourceItem { get; }

	/// <summary>
	/// Используется ли преобразование по шкале
	/// </summary>
	bool IsScaling { get; }

	/// <summary>
	/// Минимальное возможное значение по новой шкале
	/// </summary>
	float MinEu { get; }

	/// <summary>
	/// Максимальное возможное значение по новой шкале
	/// </summary>
	float MaxEu { get; }

	/// <summary>
	/// Минимальное возможное значение по старой шкале
	/// </summary>
	float MinRaw { get; }

	/// <summary>
	/// Максимальное возможное значение по старой шкале
	/// </summary>
	float MaxRaw { get; }

	/// <summary>
	/// Используемая формула
	/// </summary>
	string? Formula { get; }

	/// <summary>
	/// Тип агрегации
	/// </summary>
	TagAggregation? Aggregation { get; }

	/// <summary>
	/// Временное окно для расчета агрегированного значения
	/// </summary>
	AggregationPeriod? AggregationPeriod { get; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета агрегированного значения
	/// </summary>
	int? SourceTagId { get; }
}