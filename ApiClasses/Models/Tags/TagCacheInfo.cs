using Datalake.ApiClasses.Enums;

namespace Datalake.ApiClasses.Models.Tags;

/// <summary>
/// Базовая информация о теге для кэша
/// </summary>
public class TagCacheInfo
{
	/// <summary>
	/// Идентификатор в БД
	/// </summary>
	public required int Id { get; set; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	public required Guid Guid { get; set; }

	/// <summary>
	/// Имя
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Тип значений
	/// </summary>
	public required TagType TagType { get; set; }

	/// <summary>
	/// Тип источника значений
	/// </summary>
	public required SourceType SourceType { get; set; }

	/// <summary>
	/// Является ли тег мануальным - влияет на метод записи
	/// </summary>
	public required bool IsManual { get; set; }

	/// <summary>
	/// Коэффициент преобразования (соотношение новой и исходной шкал, заданных в настройках тега)
	/// </summary>
	public required float ScalingCoefficient { get; set; } = 1;
}
