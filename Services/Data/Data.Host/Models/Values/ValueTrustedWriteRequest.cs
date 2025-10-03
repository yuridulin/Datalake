using Datalake.Contracts.Public.Enums;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Tags;

namespace Datalake.PublicApi.Models.Values;

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
