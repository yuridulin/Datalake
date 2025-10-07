using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Datalake.Data.Application.Features.DataCollection.Commands.RestartCollection;

public interface IRestartCollectionHandler : ICommandHandler<RestartCollectionCommand, bool> { }

public class RestartCollectionHandler(
	IServiceScopeFactory serviceScopeFactory,
	IDataCollectorProcessor dataCollectorProcessor) : IRestartCollectionHandler
{
	public async Task<bool> HandleAsync(RestartCollectionCommand command, CancellationToken ct = default)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var sourcesRepository = scope.ServiceProvider.GetRequiredService<ISourcesSettingsRepository>();

		var sourcesSettings = await sourcesRepository.GetAllAsync(ct);

		await dataCollectorProcessor.RestartAsync(sourcesSettings);

		return true;
	}
}
