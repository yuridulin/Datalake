using Datalake.Domain.Entities;
using Microsoft.Extensions.Hosting;

namespace Datalake.Data.Application.Interfaces.DataCollection;

/// <summary>
/// Служба записи собранных значений в БД
/// </summary>
public interface IDataCollectorWriter : IHostedService
{
	/// <summary>
	/// Добавление новых значений в очередь на запись
	/// </summary>
	ValueTask WriteAsync(TagValue value);
}