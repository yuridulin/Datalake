using Datalake.ApiClasses.Enums;
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
			var customSources = await db.SourcesRepository.GetInfo(withCustom: true)
				.ToArrayAsync();

			Assert.Equal(customSources.Length, Enum.GetValues<CustomSource>().Length);

			var sourceInfo = await db.SourcesRepository.GetInfo(withCustom: true)
				.Where(x => x.Type == SourceType.Custom && x.Id == (int)CustomSource.Manual)
				.FirstOrDefaultAsync();

			Assert.NotNull(sourceInfo);

			// clear
			await Setup.DisposeDatabaseAsync(DbName);
		}
	}
}