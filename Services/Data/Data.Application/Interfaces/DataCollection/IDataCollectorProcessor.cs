using Datalake.Data.Application.Models.Sources;
using Datalake.Domain.Entities;

namespace Datalake.Data.Application.Interfaces.DataCollection;

public interface IDataCollectorProcessor
{
	/// <summary>
	/// Перезапускает коллекторы на основе текущих настроек
	/// </summary>
	Task RestartAsync(IEnumerable<SourceSettingsDto> sources);

	Task WriteValuesAsync(IReadOnlyCollection<TagValue> values, CancellationToken cancellationToken = default);
}