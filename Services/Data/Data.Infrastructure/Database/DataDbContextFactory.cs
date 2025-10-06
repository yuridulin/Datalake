using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Datalake.Data.Infrastructure.Database;

public class DataDbContextFactory : IDesignTimeDbContextFactory<DataDbContext>
{
	public DataDbContext CreateDbContext(string[] args)
	{
		var environment = "Migrations";

		var storage = Path.Combine(Directory.GetCurrentDirectory(), "storage", "config");
		var config = new ConfigurationBuilder()
			.SetBasePath(storage)
			.AddJsonFile("appsettings.json")
			.AddJsonFile($"appsettings.{environment}.json", optional: true)
			.Build();

		var optionsBuilder = new DbContextOptionsBuilder<DataDbContext>();
		optionsBuilder.UseNpgsql(config.GetConnectionString("Default"), options =>
		{
			options.MigrationsAssembly($"{nameof(Datalake)}.{nameof(Data)}.{nameof(Infrastructure)}");
		});

		return new(optionsBuilder.Options);
	}
}
