using Datalake.Database.Repositories;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;
using LinqToDB;
using LinqToDB.AspNet.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.Tests
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

		public static DatalakeEfContext CreateEfContext(string dbName)
		{
			string connString = ConnectionString.Replace("datalake_test", "datalake_test_" + dbName);

			var efOptions = new DbContextOptionsBuilder<DatalakeEfContext>()
				.UseNpgsql(connString, b => b.MigrationsAssembly(nameof(DatalakeContext)))
				.Options;

			var efContext = new DatalakeEfContext(efOptions);

			return efContext;
		}

		public static async Task DisposeDatabaseAsync(string dbName)
		{
			var context = CreateEfContext(dbName);
			await context.Database.EnsureDeletedAsync();
		}

		public static async Task<DatalakeContext> CreateDbContextAsync(string dbName)
		{
			var context = CreateEfContext(dbName);
			await context.Database.EnsureCreatedAsync();

			string connString = ConnectionString.Replace("datalake_test", "datalake_test_" + dbName);

			var options = new DataOptions<DatalakeContext>(
				new DataOptions()
					.UsePostgreSQL(connString)
					.UseLoggerFactory(new LoggerFactory())
			);

			DatalakeContext.SetupLinqToDB();

			var dbContext = new DatalakeContext(options);
			await dbContext.EnsureDataCreatedAsync();

			return dbContext;
		}

		public static async Task<UserAuthInfo> GetDefaultAdminAsync(this DatalakeContext db)
		{
			var userAuthInfo = await AccessRepository.AuthenticateAsync(db, new UserLoginPass
			{
				Login = "admin",
				Password = "admin",
			});

			return userAuthInfo;
		}
	}
}
