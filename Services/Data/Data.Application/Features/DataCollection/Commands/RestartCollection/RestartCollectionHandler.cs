using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.DataCollection.Commands.RestartCollection;

public interface IRestartCollectionHandler : ICommandHandler<RestartCollectionCommand, bool> { }

public class RestartCollectionHandler(
	ISourcesSettingsRepository sourcesRepository,
	IDataCollectorProcessor dataCollectorProcessor,
	ITagsStore tagsStore) : IRestartCollectionHandler
{
	public async Task<bool> HandleAsync(RestartCollectionCommand command, CancellationToken ct = default)
	{
		var sourcesSettings = await sourcesRepository.GetAllAsync(ct);

		await dataCollectorProcessor.RestartAsync(sourcesSettings);
		await tagsStore.UpdateAsync(sourcesSettings.SelectMany(s => s.Tags));

		return true;
	}
}
