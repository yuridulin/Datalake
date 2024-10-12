using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Tags;
using Datalake.ApiClasses.Models.Users;
using Datalake.Database.Repositories;
using LinqToDB;

namespace Datalake.Database.Tests.Steps
{
	public static class Step002_TagCreation
	{
		public static async Task T2_0_CreateStaticUser()
		{
			using var db = Setup.CreateDbContext();

			var userAuthInfo = await db.AccessRepository.AuthenticateAsync(new UserLoginPass
			{
				Login = "admin",
				Password = "admin",
			});

			Assert.NotNull(userAuthInfo);

			Constants.DefaultAdmin = userAuthInfo;
		}

		public static async Task T2_1_CreateManualTag()
		{
			using var db = Setup.CreateDbContext();

			/*var tagsRepository = new TagsRepository(db);

			var request = new TagCreateRequest
			{
				Name = Constants.TagName,
				TagType = TagType.Number,
				SourceId = (int)CustomSource.Manual,
			};

			Assert.NotNull(Constants.DefaultAdmin);

			int tagId = await tagsRepository.CreateAsync(Constants.DefaultAdmin, request);

			Assert.True(tagId > 0);*/
		}

		public static async Task T2_2_GetManualTag()
		{
			using var db = Setup.CreateDbContext();

			/*var tagsRepository = new TagsRepository(db);

			var tagInfo = await tagsRepository.GetInfoWithSources()
				.Where(x => x.Guid == Constants.TagGuid)
				.FirstOrDefaultAsync();

			Assert.NotNull(tagInfo);
			Assert.True(tagInfo.Name == Constants.TagName);
			Assert.True(tagInfo.SourceId == (int)CustomSource.Manual);*/
		}

		public static async Task T2_3_GetLiveValue()
		{
			var db = Setup.CreateDbContext();

			var valuesRepository = new ValuesRepository(db);

			var valuesResponses = await valuesRepository.GetValuesAsync(
			[
				new()
				{
					RequestKey = "1",
					Tags = [Constants.TagGuid]
				},
			]);

			Assert.Single(valuesResponses);
			Assert.Single(valuesResponses[0].Tags[0].Values);
		}
	}
}
