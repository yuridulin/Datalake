using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Sources;
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
	SourcesStateService stateService,
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
				=> new InopcCollector(receiverService, stateService, source, loggerFactory.CreateLogger<InopcCollector>()),

			SourceType.Datalake
				=> new OldDatalakeCollector(receiverService, stateService, source, loggerFactory.CreateLogger<OldDatalakeCollector>()),

			SourceType.DatalakeCore_v1
				=> new DatalakeCollector(receiverService, source, loggerFactory.CreateLogger<DatalakeCollector>()),

			SourceType.Custom => (CustomSource)source.Id switch
			{
				CustomSource.Calculated
					=> new CalculateCollector(source, loggerFactory.CreateLogger<CalculateCollector>()),

				CustomSource.System
					=> new SystemCollector(source, loggerFactory.CreateLogger<SystemCollector>()),

				CustomSource.Manual => null,
				_ => null
			},
			_ => null,
		};
	}
}
