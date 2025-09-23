namespace Datalake.PublicApi.Enums;

/// <summary>
/// Степень важности сообщения
/// </summary>
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
