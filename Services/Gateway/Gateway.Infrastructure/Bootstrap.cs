using Datalake.Gateway.Application.Interfaces;
using Datalake.Gateway.Application.Interfaces.Repositories;
using Datalake.Gateway.Application.Interfaces.Storage;
using Datalake.Gateway.Infrastructure.Database;
using Datalake.Gateway.Infrastructure.Database.Repositories;
using Datalake.Gateway.Infrastructure.Database.Services;
using Datalake.Gateway.Infrastructure.InMemory;
using Datalake.Shared.Application.Interfaces.AccessRules;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.Database.Schema;
using Datalake.Shared.Infrastructure.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datalake.Gateway.Infrastructure;

public static class Bootstrap
{
	public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
	{
		// БД
		var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";
		connectionString = EnvExpander.FillEnvVariables(connectionString);

		builder.Services.AddNpgsqlDataSource(connectionString);
		builder.Services.AddDbContext<GatewayDbContext>(options =>
		{
			options.UseNpgsql(connectionString, npgsql =>
			{
				npgsql.MigrationsHistoryTable(GatewaySchema.Migrations, GatewaySchema.Name);
			});
		});

		// БД: репозитории
		builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
		builder.Services.AddScoped<IUserSessionsRepository, UserSessionsRepository>();
		builder.Services.AddScoped<IUsersRepository, UsersRepository>();
		builder.Services.AddScoped<IUserAccessValuesRepository, UserAccessValuesRepository>();

		// БД: получение данных

		// Кэширование
		builder.Services.AddSingleton<ISessionsStore, SessionsStore>();
		builder.Services.AddSingleton<IUsersActivityStore, UsersActivityStore>();
		builder.Services.AddSingleton<IUsersAccessStore, UsersAccessStore>();

		// Системы

		// Настройка
		builder.Services.AddSingleton<IInfrastructureStartService, InfrastructureStartService>();

		return builder;
	}
}
