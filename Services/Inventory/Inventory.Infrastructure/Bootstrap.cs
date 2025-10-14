using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Infrastructure.Database;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Inventory.Infrastructure.Database.Initialization;
using Datalake.Inventory.Infrastructure.Database.Queries;
using Datalake.Inventory.Infrastructure.Database.Repositories;
using Datalake.Inventory.Infrastructure.InMemory.EnergoId;
using Datalake.Inventory.Infrastructure.InMemory.EnergoId.Queries;
using Datalake.Inventory.Infrastructure.InMemory.Inventory;
using Datalake.Inventory.Infrastructure.InMemory.UserAccess;
using Datalake.Inventory.Infrastructure.Interfaces;
using Datalake.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datalake.Inventory.Infrastructure;

public static class Bootstrap
{
	public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
	{
		var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";
		connectionString = EnvExpander.FillEnvVariables(connectionString);

		builder.Services.AddNpgsqlDataSource(connectionString);

		builder.Services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(connectionString));

		builder.Services.AddScoped<IUnitOfWork, DbUnitOfWork>();

		// репозитории
		builder.Services.AddScoped<IAccessRulesRepository, AccessRulesRepository>();
		builder.Services.AddScoped<IAuditRepository, AuditRepository>();
		builder.Services.AddScoped<IBlockPropertiesRepository, BlockPropertiesRepository>();
		builder.Services.AddScoped<IBlocksRepository, BlocksRepository>();
		builder.Services.AddScoped<IBlockTagsRepository, BlockTagsRepository>();
		builder.Services.AddScoped<ICalculatedAccessRulesRepository, CalculatedAccessRulesRepository>();
		builder.Services.AddScoped<IEnergoIdRepository, EnergoIdRepository>();
		builder.Services.AddScoped<ISettingsRepository, SettingsRepository>();
		builder.Services.AddScoped<ISourcesRepository, SourcesRepository>();
		builder.Services.AddScoped<ITagInputsRepository, TagInputsRepository>();
		builder.Services.AddScoped<ITagsRepository, TagRepository>();
		builder.Services.AddScoped<ITagThresholdsRepository, TagThresholdsRepository>();
		builder.Services.AddScoped<IUserGroupRelationsRepository, UserGroupRelationsRepository>();
		builder.Services.AddScoped<IUserGroupsRepository, UserGroupsRepository>();
		builder.Services.AddScoped<IUsersRepository, UsersRepository>();

		// получение данных
		builder.Services.AddScoped<IAccessRulesQueriesService, AccessRulesQueriesService>();
		builder.Services.AddScoped<IAuditQueriesService, AuditQueriesService>();
		builder.Services.AddScoped<IBlocksQueriesService, BlocksQueriesService>();
		builder.Services.AddScoped<IEnergoIdQueriesService, EnergoIdQueriesService>();
		builder.Services.AddScoped<ISettingsQueriesService, SettingsQueriesService>();
		builder.Services.AddScoped<ISourcesQueriesService, SourcesQueriesService>();
		builder.Services.AddScoped<ITagsQueriesService, TagsQueriesService>();
		builder.Services.AddScoped<IUsersGroupsQueriesService, UsersGroupsQueriesService>();
		builder.Services.AddScoped<IUsersQueriesService, UsersQueriesService>();

		// кэши
		builder.Services.AddSingleton<IInventoryCache, InventoryCache>();
		builder.Services.AddSingleton<IUserAccessCache, UserAccessCache>();
		builder.Services.AddSingleton<IEnergoIdCache, EnergoIdCache>();

		// настройка
		builder.Services.AddSingleton<IInfrastructureStartService, InfrastructureStartService>();
		builder.Services.AddSingleton<IDomainStartService, DomainStartService>();
		builder.Services.AddSingleton<IEnergoIdViewCreator, EnergoIdViewCreator>();

		// службы
		builder.Services.AddHostedService(provider => provider.GetRequiredService<IEnergoIdCache>());

		return builder;
	}
}
