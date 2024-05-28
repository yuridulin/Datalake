using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Models.Tags;
using DatalakeDatabase.Repositories;
using LinqToDB;

namespace DatalakeDatabase.Tests.Steps
{
	public static class Step002_TagCreation
	{
		public static async Task T2_1_CreateManualTag()
		{
			using var db = Setup.CreateDbContext();

			var tagsRepository = new TagsRepository(db);

			var request = new TagCreateRequest
			{
				Name = Constants.TagName,
				TagType = TagType.Number,
				SourceId = (int)CustomSource.Manual,
			};

			// add static test record
			/*int tagId = await tagsRepository.CreateAsync(request);

			Assert.True(tagId == Constants.TagId);*/
		}

		public static async Task T2_2_GetManualTag()
		{
			using var db = Setup.CreateDbContext();

			var tagsRepository = new TagsRepository(db);

			var tagInfo = await tagsRepository.GetInfoWithSources()
				.Where(x => x.Id == Constants.TagId)
				.FirstOrDefaultAsync();

			Assert.NotNull(tagInfo);
			Assert.True(tagInfo.Name == Constants.TagName);
			Assert.True(tagInfo.SourceId == (int)CustomSource.Manual);
		}

		public static async Task T2_3_GetLiveValue()
		{
			var db = Setup.CreateDbContext();

			var valuesRepository = new ValuesRepository(db);

			var valuesResponses = await valuesRepository.GetValuesAsync(
			[
				new()
				{
					Tags = [Constants.TagId]
				},
			]);

			Assert.Single(valuesResponses);
			Assert.Single(valuesResponses.First().Values);
		}
	}
}
