using DatalakeDatabase.ApiModels.Tags;
using DatalakeDatabase.Enums;
using DatalakeDatabase.Repositories;
using LinqToDB;

namespace DatalakeDatabase.Tests.Steps.Step002
{
	public static class TagCreationTests
	{
		public static async Task CreateManualTag()
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

		public static async Task GetManualTag()
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

		public static async Task GetLiveValue()
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
