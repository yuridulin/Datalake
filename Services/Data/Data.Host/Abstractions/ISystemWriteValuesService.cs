using Datalake.PublicApi.Models.Values;

namespace Datalake.DataService.Abstractions;

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