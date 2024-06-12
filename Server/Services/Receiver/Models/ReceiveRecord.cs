using Datalake.ApiClasses.Enums;

namespace Datalake.Server.Services.Receiver.Models;

/// <summary>
/// Запись о полученном из источника значении
/// </summary>
public class ReceiveRecord
{
	/// <summary>
	/// Путь, по которому доступно значение
	/// </summary>
	public string Name { get; set; } = "";

	/// <summary>
	/// Значение
	/// </summary>
	public object? Value { get; set; } = null;

	/// <summary>
	/// Тип данных значения
	/// </summary>
	public TagType Type { get; set; }

	/// <summary>
	/// Достоверность значения
	/// </summary>
	public TagQuality Quality { get; set; } = 0;
}
