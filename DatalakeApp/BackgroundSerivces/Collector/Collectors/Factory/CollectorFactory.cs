using DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;
using DatalakeApp.Services.Receiver;
using DatalakeDatabase.Enums;
using DatalakeDatabase.Models;

namespace DatalakeApp.BackgroundSerivces.Collector.Collectors.Factory;

public class CollectorFactory(ReceiverService receiverService)
{
	public ICollector? GetCollector(Source source)
	{
		return source.Type switch
		{
			SourceType.Inopc => new InopcCollector(receiverService, source),
			SourceType.Datalake => new DatalakeCollector(source),
			SourceType.Custom => (CustomSource)source.Id switch
			{
				CustomSource.Calculated => new CalculateCollector(source),
				CustomSource.System => new CalculateCollector(source),
				CustomSource.Manual => null,
				_ => null
			},
			_ => null,
		};
	}
}
