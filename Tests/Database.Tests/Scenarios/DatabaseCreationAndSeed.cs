using Datalake.PublicApi.Enums;
using LinqToDB;

namespace Datalake.Database.Tests.Scenarios
{
	public static class DatabaseCreationAndSeed
	{
		static string DbName = "seed";

		[Fact]
		public static async Task Process()
		{
			// setup
			using var context = Setup.CreateEfContext(DbName);

			await context.Database.EnsureDeletedAsync();
			await context.Database.EnsureCreatedAsync();

			using var db = await Setup.CreateDbContextAsync(DbName);

			await db.EnsureDataCreatedAsync();

			// check seed
			var customSources = await db.SourcesRepository.QueryInfo(withCustom: true)
				.ToArrayAsync();

			Assert.Equal(4, customSources.Length);

			var sourceInfo = await db.SourcesRepository.QueryInfo(withCustom: true)
				.Where(x => x.Id == (int)SourceType.Manual)
				.FirstOrDefaultAsync();

			Assert.NotNull(sourceInfo);

			// clear
			await Setup.DisposeDatabaseAsync(DbName);
		}
	}
}