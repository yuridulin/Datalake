using System.Text.Json.Serialization;

namespace Datalake.ApiClasses.Enums;

/// <summary>
/// Достоверность значения
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TagQuality
{
	/// <summary>
	/// Недостоверно
	/// </summary>
	Bad = 0,

	/// <summary>
	/// Недостоверно из-за потери связи
	/// </summary>
	Bad_NoConnect = 4,

	/// <summary>
	/// Недостоверно, потому что данные не были получены
	/// </summary>
	Bad_NoValues = 8,

	/// <summary>
	/// Недостоверно после ручного ввода
	/// </summary>
	Bad_ManualWrite = 26,

	/// <summary>
	/// Достоверно
	/// </summary>
	Good = 192,

	/// <summary>
	/// Достоверно после ручного ввода
	/// </summary>
	Good_ManualWrite = 216,

	/// <summary>
	/// Неизвестная достоверность
	/// </summary>
	Unknown = -1,
}
