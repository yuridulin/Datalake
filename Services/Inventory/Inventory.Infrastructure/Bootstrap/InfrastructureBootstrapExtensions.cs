using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Infrastructure.Cache.EnergoId;
using Datalake.Inventory.Infrastructure.Cache.Inventory;
using Datalake.Inventory.Infrastructure.Cache.UserAccess;
using Datalake.Inventory.Infrastructure.Database;
using Datalake.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datalake.Inventory.Infrastructure.Bootstrap;

public static class InfrastructureBootstrapExtensions
{
	public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
	{
		var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";
		connectionString = EnvExpander.FillEnvVariables(connectionString);

		builder.Services
			.AddNpgsqlDataSource(connectionString)
			.AddDbContext<InventoryEfContext>(options => options
				.UseNpgsql(connectionString));

		builder.Services.AddSingleton<IInventoryCache, InventoryCacheStore>();
		builder.Services.AddSingleton<IUserAccessCache, UserAccessCacheStore>();
		builder.Services.AddSingleton<IEnergoIdCache, EnergoIdCacheStore>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<EnergoIdCacheStore>());

		return builder;
	}
}
