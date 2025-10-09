using Datalake.Data.Application.Interfaces;
using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Data.Infrastructure.Database;
using Datalake.Data.Infrastructure.Database.Repositories;
using Datalake.Data.Infrastructure.Database.Services;
using Datalake.Data.Infrastructure.DataCollection;
using Datalake.Data.Infrastructure.DataCollection.Interfaces;
using Datalake.Data.Infrastructure.DataCollection.Repositories;
using Datalake.Data.Infrastructure.DataReceive;
using Datalake.Data.Infrastructure.InMemory;
using Datalake.Shared.Infrastructure;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datalake.Data.Infrastructure;

public static class Bootstrap
{
	public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
	{
		var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";
		connectionString = EnvExpander.FillEnvVariables(connectionString);

		builder.Services
			.AddDbContext<DataDbContext>(options => options.UseNpgsql(connectionString));

		builder.Services
			.AddLinqToDBContext<DataLinqToDbContext>((provider, options) =>
			{
				return options
					.UseDefaultLogging(provider)
					.UseTraceLevel(System.Diagnostics.TraceLevel.Verbose)
					.UsePostgreSQL(connectionString);
			});

		builder.Services.AddScoped<ISourcesSettingsRepository, SourcesSettingsRepository>();
		builder.Services.AddScoped<ITagsHistoryAggregationRepository, TagsHistoryAggregationRepository>();
		builder.Services.AddScoped<ITagsHistoryRepository, TagsHistoryRepository>();

		builder.Services.AddSingleton<IDataCollectorFactory, DataCollectorFactory>();
		builder.Services.AddSingleton<IDataCollectorProcessor, DataCollectorProcessor>();
		builder.Services.AddSingleton<IDataCollectorWriter, DataCollectorWriter>();

		builder.Services.AddSingleton<ICurrentValuesStore, CurrentValuesStore>();
		builder.Services.AddSingleton<ITagsStore, TagsStore>();
		builder.Services.AddSingleton<IDataCollectionErrorsStore, DataCollectionErrorsStore>();

		builder.Services.AddSingleton<IReceiverService, ReceiverService>();

		builder.Services.AddSingleton<DbInitializer>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<DbInitializer>());

		return builder;
	}
}
