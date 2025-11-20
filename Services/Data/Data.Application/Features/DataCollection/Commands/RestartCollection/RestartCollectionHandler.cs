using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Data.Application.Interfaces.Storage;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Application.Features.DataCollection.Commands.RestartCollection;

public interface IRestartCollectionHandler : ICommandHandler<RestartCollectionCommand, bool> { }

public class RestartCollectionHandler(
	ISourcesQueriesService sourcesRepository,
	IDataCollectorProcessor dataCollectorProcessor,
	ILogger<RestartCollectionHandler> logger,
	ITagsSettingsStore tagsStore) : IRestartCollectionHandler
{
	public async Task<bool> HandleAsync(RestartCollectionCommand command, CancellationToken ct = default)
	{
		try
		{
			var sourcesSettings = await sourcesRepository.GetAllAsync(ct);

			await dataCollectorProcessor.RestartAsync(sourcesSettings);
			await tagsStore.UpdateAsync(sourcesSettings.SelectMany(s => s.Tags));

			return true;
		}
		catch (OperationCanceledException)
		{
			logger.LogWarning("Ошибка при получении источников данных с тегами: операция отменена");
			return false;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка при получении источников данных с тегами");
			throw;
		}
	}
}
