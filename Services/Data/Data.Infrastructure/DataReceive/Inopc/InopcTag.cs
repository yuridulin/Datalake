using Datalake.Data.Infrastructure.DataReceive.Inopc.Enums;

namespace Datalake.Data.Infrastructure.DataReceive.Inopc;

/// <summary>
/// Объект данных INOPC
/// </summary>
public class InopcTag
{
	/// <summary>
	/// Значение
	/// </summary>
	public object? Value { get; set; } = null;

	/// <summary>
	/// Достоверность значения
	/// </summary>
	public InopcTagQuality Quality { get; set; } = 0;

	/// <summary>
	/// Путь, по которому значение получено
	/// </summary>
	public string Name { get; set; } = "";

	/// <summary>
	/// OPC-идентификатор значения
	/// </summary>
	public uint TagHandle { get; set; } = 0;

	/// <summary>
	/// Тип значения
	/// </summary>
	public InopcTagType Type { get; set; }
}
