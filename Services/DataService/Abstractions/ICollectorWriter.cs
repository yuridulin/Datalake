using Datalake.PublicApi.Models.Values;

namespace Datalake.DataService.Abstractions;

/// <summary>
/// Служба записи собранных значений в БД
/// </summary>
public interface ICollectorWriter : IHostedService
{
	/// <summary>
	/// Добавление новых значений в очередь на запись
	/// </summary>
	/// <param name="values">Список новых значений</param>
	void AddToQueue(IEnumerable<ValueWriteRequest> values);
}