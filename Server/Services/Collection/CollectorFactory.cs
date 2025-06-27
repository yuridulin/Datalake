using Datalake.Database;
using Datalake.Database.InMemory;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Collection.Collectors;
using Datalake.Server.Services.Maintenance;
using Datalake.Server.Services.Receiver;

namespace Datalake.Server.Services.Collection;

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
				=> new InopcCollector(
					receiverService,
					source,
					sourcesStateService,
					loggerFactory.CreateLogger<InopcCollector>()),

			SourceType.Datalake
				=> new OldDatalakeCollector(
					receiverService,
					source,
					sourcesStateService,
					loggerFactory.CreateLogger<OldDatalakeCollector>()),

			SourceType.Datalake_v2
				=> new DatalakeCollector(
					receiverService,
					source,
					sourcesStateService,
					loggerFactory.CreateLogger<DatalakeCollector>()),

			SourceType.Calculated
				=> new CalculateCollector(
					serviceProvider.GetRequiredService<DatalakeCurrentValuesStore>(),
					tagsStateService,
					source,
					sourcesStateService,
					loggerFactory.CreateLogger<CalculateCollector>()),

			SourceType.System
				=> new SystemCollector(
				source,
				sourcesStateService,
				loggerFactory.CreateLogger<SystemCollector>()),

			SourceType.Aggregated
				=> new AggregateCollector(
					serviceProvider.CreateScope().ServiceProvider.GetRequiredService<DatalakeContext>(),
					tagsStateService,
					source,
					sourcesStateService,
					loggerFactory.CreateLogger<AggregateCollector>()),

			_ => null,
		};
	}
}
