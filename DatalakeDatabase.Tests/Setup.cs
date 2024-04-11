using LinqToDB;
using LinqToDB.AspNet.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DatalakeDatabase.Tests
{
	public static class Setup
	{
		public static readonly string ConnectionString = 
			"Server=10.208.4.113; " +
			"Port=5432; " +
			"Database=datalake_test; " +
			"User Id=postgres; " +
			"Password=postgres; " +
			"Pooling=true; MinPoolSize=10; MaxPoolSize=100;";

		public static DatalakeEfContext CreateEfContext()
		{
			var efOptions = new DbContextOptionsBuilder<DatalakeEfContext>()
				.UseNpgsql(ConnectionString, b => b.MigrationsAssembly(nameof(DatalakeContext)))
				.Options;

			var efContext = new DatalakeEfContext(efOptions);

			return efContext;
		}

		public static DatalakeContext CreateDbContext()
		{
			var options = new DataOptions<DatalakeContext>(
				new DataOptions()
					.UsePostgreSQL(ConnectionString)
					.UseLoggerFactory(
						LoggerFactory.Create(builder => 
							builder.AddDebug()
						)
					)
			);

			DatalakeContext.SetupLinqToDB();

			var dbContext = new DatalakeContext(options);

			return dbContext;
		}
	}
}
