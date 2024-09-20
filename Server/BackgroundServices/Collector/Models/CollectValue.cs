using Datalake.ApiClasses.Enums;

namespace Datalake.Server.BackgroundServices.Collector.Models;

/// <summary>
/// Информация о полученном значении
/// </summary>
public struct CollectValue
{
	/// <summary>
	/// Дата получения значения
	/// </summary>
	public DateTime DateTime { get; set; }

	/// <summary>
	/// Идентификатор тега, для которого предназначено это значение
	/// </summary>
	public Guid Guid { get; set; }

	/// <summary>
	/// Путь, по которому было получено значение
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Значение
	/// </summary>
	public object? Value { get; set; }

	/// <summary>
	/// Достоверность значения
	/// </summary>
	public TagQuality Quality { get; set; }
}