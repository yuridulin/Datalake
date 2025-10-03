using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Datalake.Inventory.Infrastructure.Database;

public class InventoryDbContextFactory : IDesignTimeDbContextFactory<InventoryDbContext>
{
	public InventoryDbContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<InventoryDbContext>();

		// Можно взять строку подключения из env или захардкодить для миграций
		var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION")
				?? "Host=localhost;Database=inventory;Username=postgres;Password=postgres";

		optionsBuilder.UseNpgsql(connectionString);

		return new InventoryDbContext(optionsBuilder.Options);
	}
}

