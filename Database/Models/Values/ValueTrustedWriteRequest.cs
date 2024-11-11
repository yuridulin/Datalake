using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Models.Tags;

namespace Datalake.Database.Models.Values;

/// <summary>
/// Данные запроса на ввод значения
/// </summary>
public class ValueTrustedWriteRequest
{
	/// <summary>
	/// Информация о теге, значение которого записывается
	/// </summary>
	public required TagCacheInfo Tag { get; set; }

	/// <summary>
	/// Новое значение
	/// </summary>
	public object? Value { get; set; }

	/// <summary>
	/// Дата, на которую будет записано значение
	/// </summary>
	public DateTime? Date { get; set; } = DateFormats.GetCurrentDateTime();

	/// <summary>
	/// Флаг достоверности нового значения
	/// </summary>
	public TagQuality? Quality { get; set; }
}
