using Datalake.Database.Enums;

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

	/// <inheritdoc/>
	public override readonly int GetHashCode()
	{
		int hashQuality = Quality.GetHashCode();
		int hashValue = Value?.GetHashCode() ?? 0;

		return hashQuality ^ hashValue;
	}

	/// <inheritdoc/>
	public override readonly bool Equals(object? obj)
	{
		return obj is CollectValue history && GetHashCode() == history.GetHashCode();
	}

	/// <inheritdoc/>
	public static bool operator ==(CollectValue left, CollectValue right)
	{
		return left.Equals(right);
	}

	/// <inheritdoc/>
	public static bool operator !=(CollectValue left, CollectValue right)
	{
		return !(left == right);
	}
}