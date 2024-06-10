using System.Text.Json.Serialization;

namespace DatalakeApiClasses.Enums;

/// <summary>
/// Тип данных
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TagType
{
	/// <summary>
	/// Строка
	/// </summary>
	String = 0,

	/// <summary>
	/// Число
	/// </summary>
	Number = 1,

	/// <summary>
	/// Логическое значение
	/// </summary>
	Boolean = 2,
}
