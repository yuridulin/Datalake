using Datalake.Data.Application.Models.Sources;

namespace Datalake.Data.Application.Interfaces.DataCollection;

public interface IDataCollectorProcessor
{
	/// <summary>
	/// Перезапускает коллекторы на основе текущих настроек
	/// </summary>
	Task RestartAsync(IEnumerable<SourceSettingsDto> sourcesSettings);
}