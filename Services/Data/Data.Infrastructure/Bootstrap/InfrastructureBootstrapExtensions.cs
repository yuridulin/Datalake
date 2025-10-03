using Datalake.Data.Infrastructure.Database;
using Datalake.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datalake.Data.Infrastructure.Bootstrap;

public static class InfrastructureBootstrapExtensions
{
	public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
	{
		var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";
		connectionString = EnvExpander.FillEnvVariables(connectionString);

		builder.Services
			.AddDbContext<DataDbContext>(options => options.UseNpgsql(connectionString));

		return builder;
	}
}
