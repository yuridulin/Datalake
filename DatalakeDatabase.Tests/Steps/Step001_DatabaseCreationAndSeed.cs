using DatalakeDatabase.Enums;
using DatalakeDatabase.Repositories;
using LinqToDB;

namespace DatalakeDatabase.Tests.Steps
{
	public static class Step001_DatabaseCreationAndSeed
	{
		public static async Task CreationTest()
		{
			using var context = Setup.CreateEfContext();

			await context.Database.EnsureDeletedAsync();
			await context.Database.EnsureCreatedAsync();
		}

		public static async Task SeedTest()
		{
			DatalakeContext.SetupLinqToDB();
			using var db = Setup.CreateDbContext();

			await db.EnsureDataCreatedAsync();

			var customSources = await db.Sources
				.ToArrayAsync();

			Assert.Equal(customSources.Length, Enum.GetValues<CustomSource>().Length);
		}

		public static async Task GetManualSource()
		{
			using var db = Setup.CreateDbContext();

			var sourcesRepository = new SourcesRepository(db);
			var sourceInfo = await sourcesRepository.GetSources()
				.Where(x => x.Type == SourceType.Custom && x.Id == (int)CustomSource.Manual)
				.FirstOrDefaultAsync();

			Assert.NotNull(sourceInfo);
		}
	}
}