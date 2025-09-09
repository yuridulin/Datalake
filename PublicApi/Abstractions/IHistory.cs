using Datalake.PublicApi.Enums;

namespace Datalake.PublicApi.Abstractions;

/// <summary>
/// Запись значения
/// </summary>
public interface IHistory
{
	/// <summary>
	/// Дата
	/// </summary>
	DateTime Date { get; set; }

	/// <summary>
	/// Текстовое представление
	/// </summary>
	string? Text { get; set; }

	/// <summary>
	/// Числовое представление
	/// </summary>
	float? Number { get; set; }

	/// <summary>
	/// Качество
	/// </summary>
	TagQuality Quality { get; set; }
}