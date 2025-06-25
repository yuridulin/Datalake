using Datalake.Database;
using Datalake.Database.InMemory;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Collectors;
using Datalake.Server.Services.Receiver;
using Datalake.Server.Services.StateManager;

namespace Datalake.Server.BackgroundServices.Collector;

/// <summary>
/// Получение нужного сборщика данных для выбранного источника
/// </summary>
public class CollectorFactory(
	ReceiverService receiverService,
	SourcesStateService sourcesStateService,
	TagsStateService tagsStateService,
	IServiceProvider serviceProvider,
	ILoggerFactory loggerFactory)
{
	/// <summary>
	/// Получение сборщика для источника
	/// </summary>
	/// <param name="source">Выбранный источник данных</param>
	/// <returns>Новый экземпляр подходящего сборщика</returns>
	public ICollector? GetCollector(SourceWithTagsInfo source)
	{
		return source.Type switch
		{
			SourceType.Inopc
				=> new InopcCollector(receiverService, sourcesStateService, source, loggerFactory.CreateLogger<InopcCollector>()),

			SourceType.Datalake
				=> new OldDatalakeCollector(receiverService, sourcesStateService, source, loggerFactory.CreateLogger<OldDatalakeCollector>()),

			SourceType.Datalake_v2
				=> new DatalakeCollector(receiverService, source, loggerFactory.CreateLogger<DatalakeCollector>()),

			SourceType.Calculated
				=> new CalculateCollector(serviceProvider.GetRequiredService<DatalakeCurrentValuesStore>(), source, tagsStateService, loggerFactory.CreateLogger<CalculateCollector>()),

			SourceType.System
				=> new SystemCollector(source, loggerFactory.CreateLogger<SystemCollector>()),

			SourceType.Aggregated
				=> new AggregateCollector(
					serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatalakeContext>(),
					source,
					tagsStateService,
					loggerFactory.CreateLogger<AggregateCollector>()),

			_ => null,
		};
	}
}
