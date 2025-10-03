using Datalake.PublicApi.Models.Sources;

namespace Datalake.Data.Host.Services.Collection;

/// <summary>
/// Служба управления сборщиками данных для имеющихся источников данных
/// </summary>
public interface ICollectorProcessor : IHostedService
{
	/// <summary>
	/// Обновление списка источников данных
	/// </summary>
	/// <param name="newSources">Новый список источников</param>
	Task UpdateAsync(IEnumerable<SourceWithTagsInfo> newSources);
}