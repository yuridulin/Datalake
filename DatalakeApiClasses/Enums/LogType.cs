using System.Text.Json.Serialization;

namespace DatalakeApiClasses.Enums;

/// <summary>
/// Степень важности сообщения
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum LogType
{
	/// <summary>
	/// Отладка
	/// </summary>
	Trace = 0,

	/// <summary>
	/// Информирующее
	/// </summary>
	Information = 1,

	/// <summary>
	/// Сигнализация о успешном выполнении операции
	/// </summary>
	Success = 2,

	/// <summary>
	/// Предупреждение
	/// </summary>
	Warning = 3,

	/// <summary>
	/// Ошибка
	/// </summary>
	Error = 4,
}
