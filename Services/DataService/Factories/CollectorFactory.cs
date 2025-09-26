using Datalake.DataService.Abstractions;
using Datalake.DataService.Services.Collection.External;
using Datalake.DataService.Services.Collection.Internal;
using Datalake.PrivateApi.Attributes;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.DataService.Factories;

[Singleton]
public class CollectorFactory(
	IServiceProvider provider,
	ILogger<CollectorFactory> logger) : ICollectorFactory
{
	public ICollector? GetCollector(SourceWithTagsInfo source)
	{
		ICollector? collector = source.Type switch
		{
			SourceType.Inopc
				=> ActivatorUtilities.CreateInstance<InopcCollector>(provider, source),

			SourceType.Datalake
				=> ActivatorUtilities.CreateInstance<DatalakeCollector>(provider, source),

			SourceType.Calculated
				=> ActivatorUtilities.CreateInstance<CalculateCollector>(provider, source),

			SourceType.System
				=> ActivatorUtilities.CreateInstance<SystemCollector>(provider, source),

			SourceType.Aggregated
				=> ActivatorUtilities.CreateInstance<AggregateCollector>(provider, source),

			SourceType.Manual
				=> ActivatorUtilities.CreateInstance<ManualCollector>(provider, source),
			
			_ => null,
		};

		if (collector == null)
			logger.LogWarning("Указанный тип источника данных не поддерживается: {type}", source.Type);

		return collector;
	}
}
