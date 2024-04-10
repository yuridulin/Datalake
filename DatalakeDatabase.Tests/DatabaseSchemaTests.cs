using DatalakeDatabase.ApiModels.Sources;
using DatalakeDatabase.ApiModels.Tags;
using DatalakeDatabase.Enums;
using DatalakeDatabase.Repositories;
using DatalakeDatabase.Tests.Attributes;
using LinqToDB;

namespace DatalakeDatabase.Tests
{
	[TestCaseOrderer("DatalakeDatabase.Tests.Attributes.PriorityOrderer", "DatalakeDatabase.Tests")]
	public class DatabaseSchemaTests
	{
		[Fact, TestPriority(0)]
		public async Task CreationTest()
		{
			using var context = Setup.CreateEfContext();

			await context.Database.EnsureDeletedAsync();
			await context.Database.EnsureCreatedAsync();
		}

		[Fact, TestPriority(1)]
		public async Task SeedTest()
		{
			using var db = Setup.CreateDbContext();

			await db.EnsureDataCreatedAsync();

			var customSources = await db.Sources
				.ToArrayAsync();

			Assert.Equal(customSources.Length, Enum.GetValues<CustomSource>().Length);
		}

		[Fact, TestPriority(2)]
		public async Task GetManualSource()
		{
			using var db = Setup.CreateDbContext();

			var sourcesRepository = new SourcesRepository(db);
			var sourceInfo = await sourcesRepository.GetSources()
				.Where(x => x.Type == SourceType.Custom && x.Id == (int)CustomSource.Manual)
				.FirstOrDefaultAsync();

			Assert.NotNull(sourceInfo);
		}

		[Fact, TestPriority(3)]
		public async Task CreateManualTag()
		{
			using var db = Setup.CreateDbContext();

			var tagsRepository = new TagsRepository(db);

			var request = new TagInfo
			{
				Name = "test",
				SourceInfo = new TagInfo.TagSourceInfo
				{
					Id = (int)CustomSource.Manual,
				}
			};

			int tagId = await tagsRepository.CreateAsync(request);

			Assert.True(tagId == 1);
		}

		[Fact, TestPriority(4)]
		public async Task GetManualTag()
		{
			using var db = Setup.CreateDbContext();

			var tagsRepository = new TagsRepository(db);

			var tagInfo = await tagsRepository.GetTagsWithSources()
				.Where(x => x.Id == 1)
				.FirstOrDefaultAsync();

			Assert.NotNull(tagInfo);
			Assert.True(tagInfo.Name == "test");

			Assert.NotNull(tagInfo.SourceInfo);
			Assert.True(tagInfo.SourceInfo.Id == (int)CustomSource.Manual);
		}

		[Fact, TestPriority(5)]
		public async Task GetLiveValue()
		{
			var db = Setup.CreateDbContext();

			var valuesRepository = new ValuesRepository(db);

			var valuesResponses = await valuesRepository.GetValuesAsync(
			[
				new()
				{
					Tags = [1]
				},
			]);

			Assert.Single(valuesResponses);
			Assert.Single(valuesResponses.First().Values);
		}
	}
}