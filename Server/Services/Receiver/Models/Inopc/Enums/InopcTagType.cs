namespace DatalakeServer.Services.Receiver.Models.Inopc.Enums;

/// <summary>
/// Тип значения в INOPC
/// </summary>
public enum InopcTagType
{
	/// <summary>
	/// Строка
	/// </summary>
	String,

	/// <summary>
	/// Число
	/// </summary>
	Number,

	/// <summary>
	/// Логическое
	/// </summary>
	Boolean,

	/// <summary>
	/// Вычисляемые
	/// </summary>
	Computed,
}
