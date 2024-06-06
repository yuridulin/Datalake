using DatalakeApiClasses.Enums;
using DatalakeDatabase.Models;
using DatalakeServer.BackgroundServices.Collector.Collectors.Abstractions;
using DatalakeServer.Services.Receiver;

namespace DatalakeServer.BackgroundServices.Collector.Collectors.Factory;

public class CollectorFactory(ReceiverService receiverService)
{
	private ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
	{
		builder.AddDebug();
	});

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
