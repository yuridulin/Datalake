using Datalake.Domain.ValueObjects;
using Microsoft.Extensions.Hosting;

namespace Datalake.Data.Application.DataCollection.Interfaces;

/// <summary>
/// Служба записи собранных значений в БД
/// </summary>
public interface IDataWriterService : IHostedService
{
	/// <summary>
	/// Добавление новых значений в очередь на запись
	/// </summary>
	/// <param name="values">Список новых значений</param>
	void AddToQueue(IEnumerable<TagHistory> values);
}