using Datalake.Data.Application.Interfaces;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Data.Application.Interfaces.Storage;
using Datalake.Data.Infrastructure.Database;
using Datalake.Data.Infrastructure.Database.QueriesServices;
using Datalake.Data.Infrastructure.Database.Repositories;
using Datalake.Data.Infrastructure.Database.Services;
using Datalake.Data.Infrastructure.DataCollection;
using Datalake.Data.Infrastructure.DataCollection.Interfaces;
using Datalake.Data.Infrastructure.DataReceive;
using Datalake.Data.Infrastructure.InMemory;
using Datalake.Shared.Application.Interfaces.AccessRules;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.InMemory;
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
		// БД
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

		// БД: репозитории
		builder.Services.AddScoped<ISourcesRepository, SourcesRepository>();
		builder.Services.AddScoped<ITagsValuesRepository, TagsValuesRepository>();
		builder.Services.AddScoped<ITagsValuesAggregationRepository, TagsValuesAggregationRepository>();
		builder.Services.AddScoped<IUserAccessRepository, UserAccessRepository>();

		// БД: получение данных
		builder.Services.AddScoped<ISourcesQueriesService, SourcesQueriesService>();

		// Кэширование
		builder.Services.AddSingleton<IValuesStore, ValuesStore>();
		builder.Services.AddSingleton<ITagsSettingsStore, TagsSettingsStore>();
		builder.Services.AddSingleton<ITagsUsageStore, TagsUsageStore>();
		builder.Services.AddSingleton<ITagsCollectionStatusStore, TagsCollectionStatusStore>();
		builder.Services.AddSingleton<ISourcesActivityStore, SourcesActivityStore>();
		builder.Services.AddSingleton<IUsersAccessStore, UsersAccessStore>();

		// Системы
		builder.Services.AddSingleton<IReceiverService, ReceiverService>();
		builder.Services.AddSingleton<IDataCollectorFactory, DataCollectorFactory>();
		builder.Services.AddSingleton<IDataCollectorProcessor, DataCollectorProcessor>();
		builder.Services.AddSingleton<IDataCollectorWriter, DataCollectorWriter>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<IDataCollectorWriter>());

		// Настройка
		builder.Services.AddSingleton<IInfrastructureStartService, InfrastructureStartService>();

		return builder;
	}
}
