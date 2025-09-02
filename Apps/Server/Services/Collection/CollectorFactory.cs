using Datalake.Database;
using Datalake.Database.InMemory.Stores;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Collection.Collectors;
using Datalake.Server.Services.Collection.External;
using Datalake.Server.Services.Collection.Internal;
using Datalake.Server.Services.Maintenance;
using Datalake.Server.Services.Receiver;

namespace Datalake.Server.Services.Collection;

/// <summary>
/// Получение нужного сборщика данных для выбранного источника
/// </summary>
public class CollectorFactory(
	ReceiverService receiverService,
	DatalakeCurrentValuesStore currentValuesStore,
	SourcesStateService sourcesStateService,
	TagsStateService tagsStateService,
	TagsReceiveStateService receiveStateService,
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
				=> new DatalakeCollector(
					source,
					sourcesStateService,
					loggerFactory.CreateLogger<DatalakeCollector>()),

			SourceType.Calculated
				=> new CalculateCollector(
					currentValuesStore,
					tagsStateService,
					source,
					sourcesStateService,
					receiveStateService,
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
					receiveStateService,
					loggerFactory.CreateLogger<AggregateCollector>()),

			SourceType.Manual
				=> new ManualCollector(
					source,
					sourcesStateService,
					loggerFactory.CreateLogger<ManualCollector>()),

			_ => null,
		};
	}
}
