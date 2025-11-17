using Datalake.Data.Application.Interfaces;
using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Data.Infrastructure.Database;
using Datalake.Data.Infrastructure.Database.QueriesServices;
using Datalake.Data.Infrastructure.Database.Repositories;
using Datalake.Data.Infrastructure.Database.Services;
using Datalake.Data.Infrastructure.DataCollection;
using Datalake.Data.Infrastructure.DataCollection.Interfaces;
using Datalake.Data.Infrastructure.DataReceive;
using Datalake.Data.Infrastructure.InMemory;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.Schema;
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

		builder.Services.AddDbContext<DataDbContext>(options =>
		{
			options.UseNpgsql(connectionString, npgsql =>
			{
				npgsql.MigrationsHistoryTable(DataSchema.Migrations, DataSchema.Name);
			});
		});

		builder.Services.AddLinqToDBContext<DataDbLinqContext>((provider, options) =>
		{
			return options
				.UseDefaultLogging(provider)
				.UseTraceLevel(System.Diagnostics.TraceLevel.Verbose)
				.UsePostgreSQL(connectionString);
		});

		builder.Services.AddSingleton<IUserAccessStore, UserAccessStore>();

		builder.Services.AddScoped<ISourcesRepository, SourcesRepository>();
		builder.Services.AddScoped<ISourcesQueriesService, SourcesQueriesService>();
		builder.Services.AddScoped<ITagsValuesRepository, TagsValuesRepository>();
		builder.Services.AddScoped<ITagsValuesAggregationRepository, TagsValuesAggregationRepository>();

		builder.Services.AddSingleton<IDataCollectorFactory, DataCollectorFactory>();
		builder.Services.AddSingleton<IDataCollectorProcessor, DataCollectorProcessor>();
		builder.Services.AddSingleton<IDataCollectorWriter, DataCollectorWriter>();

		builder.Services.AddSingleton<IValuesStore, ValuesStore>();
		builder.Services.AddSingleton<ITagsSettingsStore, TagsSettingsStore>();
		builder.Services.AddSingleton<ITagsUsageStore, TagsUsageStore>();
		builder.Services.AddSingleton<ITagsCollectionStatusStore, TagsCollectionStatusStore>();
		builder.Services.AddSingleton<ISourcesActivityStore, SourcesActivityStore>();

		builder.Services.AddSingleton<IReceiverService, ReceiverService>();

		builder.Services.AddHostedService<DataDbStartupService>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<IDataCollectorWriter>());

		return builder;
	}
}
