using Datalake.Gateway.Application.Interfaces;
using Datalake.Gateway.Application.Interfaces.Repositories;
using Datalake.Gateway.Infrastructure.Database;
using Datalake.Gateway.Infrastructure.Database.Repositories;
using Datalake.Gateway.Infrastructure.Database.Services;
using Datalake.Gateway.Infrastructure.InMemory;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datalake.Gateway.Infrastructure;

public static class Bootstrap
{
	public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
	{
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

		builder.Services.AddSingleton<ISessionsCache, MemorySessionsCache>();
		builder.Services.AddSingleton<IUsersActivityService, UsersActivityService>();

		builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
		builder.Services.AddScoped<IUserSessionsRepository, UserSessionsRepository>();
		builder.Services.AddScoped<IUsersRepository, UsersRepository>();

		builder.Services.AddScoped<IUserAccessService, UserAccessService>();

		builder.Services.AddHostedService<GatewayDbStartupService>();

		return builder;
	}
}
