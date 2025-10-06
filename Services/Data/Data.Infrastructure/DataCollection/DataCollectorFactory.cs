using Datalake.Contracts.Public.Enums;
using Datalake.Data.Application.DataCollection.Interfaces;
using Datalake.Data.Application.DataCollection.Models;
using Datalake.Data.Infrastructure.DataCollection.Internal;
using Datalake.Data.Infrastructure.DataCollection.Remote;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection;

[Singleton]
public class DataCollectorFactory(
	IServiceProvider provider,
	ILogger<DataCollectorFactory> logger) : IDataCollectorFactory
{
	public IDataCollector? Create(SourceSettingsDto source)
	{
		IDataCollector? collector = source.SourceType switch
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
			logger.LogWarning("Указанный тип источника данных не поддерживается: {type}", source.SourceType);

		return collector;
	}
}
