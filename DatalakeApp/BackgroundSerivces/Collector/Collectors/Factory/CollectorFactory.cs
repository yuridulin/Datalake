using DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;
using DatalakeApp.Services.Receiver;
using DatalakeDatabase.Enums;
using DatalakeDatabase.Models;

namespace DatalakeApp.BackgroundSerivces.Collector.Collectors.Factory;

public class CollectorFactory(ReceiverService receiverService, ILoggerFactory loggerFactory)
{
	public ICollector? GetCollector(Source source)
	{
		return source.Type switch
		{
			SourceType.Inopc => new InopcCollector(receiverService, source, loggerFactory.CreateLogger<InopcCollector>()),
			SourceType.Datalake => new DatalakeCollector(receiverService, source, loggerFactory.CreateLogger<DatalakeCollector>()),
			SourceType.Custom => (CustomSource)source.Id switch
			{
				CustomSource.Calculated => new CalculateCollector(source, loggerFactory.CreateLogger<CalculateCollector>()),
				CustomSource.System => new SystemCollector(source, loggerFactory.CreateLogger<SystemCollector>()),
				CustomSource.Manual => null,
				_ => null
			},
			_ => null,
		};
	}
}
