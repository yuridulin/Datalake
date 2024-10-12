using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Sources;
using Datalake.Database.Utilities;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Collectors;
using Datalake.Server.Services.Receiver;

namespace Datalake.Server.BackgroundServices.Collector;

/// <summary>
/// Получение нужного сборщика данных для выбранного источника
/// </summary>
/// <param name="receiverService">Служба запроса данных из источника</param>
public class CollectorFactory(ReceiverService receiverService)
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
				=> new InopcCollector(receiverService, source, LogManager.CreateLogger<InopcCollector>()),

			SourceType.Datalake
				=> new OldDatalakeCollector(receiverService, source, LogManager.CreateLogger<OldDatalakeCollector>()),

			SourceType.DatalakeCore_v1
				=> new DatalakeCollector(receiverService, source, LogManager.CreateLogger<DatalakeCollector>()),

			SourceType.Custom => (CustomSource)source.Id switch
			{
				CustomSource.Calculated
					=> new CalculateCollector(source, LogManager.CreateLogger<CalculateCollector>()),

				CustomSource.System
					=> new SystemCollector(source, LogManager.CreateLogger<SystemCollector>()),

				CustomSource.Manual => null,
				_ => null
			},
			_ => null,
		};
	}
}
