using Datalake.Data.Infrastructure.Database;
using Datalake.Shared.Infrastructure;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
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
			.AddDbContext<DataEfContext>(options => options.UseNpgsql(connectionString));

		builder.Services
			.AddLinqToDBContext<DataLinqToDbContext>((provider, options) =>
			{
				return options
					.UseDefaultLogging(provider)
					.UseTraceLevel(System.Diagnostics.TraceLevel.Verbose)
					.UsePostgreSQL(connectionString);
			});

		return builder;
	}
}
