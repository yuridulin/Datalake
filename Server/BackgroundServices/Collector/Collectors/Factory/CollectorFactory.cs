using Datalake.ApiClasses.Enums;
using Datalake.Database.Models;
using DatalakeServer.BackgroundServices.Collector.Collectors.Abstractions;
using DatalakeServer.Services.Receiver;

namespace DatalakeServer.BackgroundServices.Collector.Collectors.Factory;

/// <summary>
/// Получение нужного сборщика данных для выбранного источника
/// </summary>
/// <param name="receiverService">Служба запроса данных из источника</param>
public class CollectorFactory(ReceiverService receiverService)
{
	private ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
	{
		builder.AddDebug();
	});

	/// <summary>
	/// Получение сборщика для источника
	/// </summary>
	/// <param name="source">Выбранный источник данных</param>
	/// <returns>Новый экземпляр подходящего сборщика</returns>
	public ICollector? GetCollector(Source source)
	{
		return source.Type switch
		{
			SourceType.Inopc => new InopcCollector(receiverService, source, _loggerFactory.CreateLogger<InopcCollector>()),
			SourceType.Datalake => new DatalakeCollector(receiverService, source, _loggerFactory.CreateLogger<DatalakeCollector>()),
			SourceType.Custom => (CustomSource)source.Id switch
			{
				CustomSource.Calculated => new CalculateCollector(source, _loggerFactory.CreateLogger<CalculateCollector>()),
				CustomSource.System => new SystemCollector(source, _loggerFactory.CreateLogger<SystemCollector>()),
				CustomSource.Manual => null,
				_ => null
			},
			_ => null,
		};
	}
}
