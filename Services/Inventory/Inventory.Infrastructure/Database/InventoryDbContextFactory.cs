using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Datalake.Inventory.Infrastructure.Database;

/// <summary>
/// Создание подключения для миграций. Строка подключения указана в отдельном конфиге
/// </summary>
public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
	public InventoryDbContext CreateDbContext(string[] args)
	{
		var environment = "Migrations";

		var storage = Path.Combine(Directory.GetCurrentDirectory(), "storage", "config");
		var config = new ConfigurationBuilder()
			.SetBasePath(storage)
			.AddJsonFile("appsettings.json")
			.AddJsonFile($"appsettings.{environment}.json", optional: true)
			.Build();

		var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();
		optionsBuilder.UseNpgsql(config.GetConnectionString("Default"));

		return new(optionsBuilder.Options);
	}
}
