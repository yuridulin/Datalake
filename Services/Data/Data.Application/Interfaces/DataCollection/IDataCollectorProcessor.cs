using Datalake.Data.Application.Models;
using Datalake.Data.Application.Models.Sources;

namespace Datalake.Data.Application.Interfaces.DataCollection;

public interface IDataCollectorProcessor
{
	/// <summary>
	/// Перезапускает коллекторы на основе текущих настроек
	/// </summary>
	Task RestartAsync(IEnumerable<SourceSettingsDto> sourceSettings);

	/// <summary>
	/// Текущее состояние всех коллекторов
	/// </summary>
	IReadOnlyCollection<DataCollectorStatus> GetCollectorsStatus();
}