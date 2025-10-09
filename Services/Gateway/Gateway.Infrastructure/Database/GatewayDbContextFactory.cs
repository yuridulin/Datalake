using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Datalake.Gateway.Infrastructure.Database;

public class GatewayDbContextFactory : IDesignTimeDbContextFactory<GatewayDbContext>
{
	public GatewayDbContext CreateDbContext(string[] args)
	{
		var environment = "Migrations";

		var storage = Path.Combine(Directory.GetCurrentDirectory(), "storage", "config");
		var config = new ConfigurationBuilder()
			.SetBasePath(storage)
			.AddJsonFile("appsettings.json")
			.AddJsonFile($"appsettings.{environment}.json", optional: true)
			.Build();

		var optionsBuilder = new DbContextOptionsBuilder<GatewayDbContext>();
		optionsBuilder.UseNpgsql(config.GetConnectionString("Default"), options =>
		{
			options.MigrationsAssembly($"{nameof(Datalake)}.{nameof(Gateway)}.{nameof(Infrastructure)}");
		});

		return new(optionsBuilder.Options);
	}
}
