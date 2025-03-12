using Datalake.PublicApi.Models.Values;

namespace Datalake.Server.BackgroundServices.Collector.Models;

/// <summary>
/// Информация о полученном значении
/// </summary>
public class CollectValue : ValueWriteRequest
{
	/// <inheritdoc/>
	public override int GetHashCode()
	{
		int hashQuality = Quality.GetHashCode();
		int hashValue = Value?.GetHashCode() ?? 0;

		return hashQuality ^ hashValue;
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
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