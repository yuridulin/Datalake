using Datalake.Data.Host.Models.Values;

namespace Datalake.Data.Host.Abstractions;

/// <summary>
/// Служба для записи собранных значений
/// </summary>
public interface ISystemWriteValuesService
{
	/// <summary>
	/// Запись входящих значений. Значения будут проверены на новизну, запишутся только новые
	/// </summary>
	/// <param name="requests">Список входящих значений</param>
	Task WriteAsync(IEnumerable<ValueWriteRequest> requests);
}