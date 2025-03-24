using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.Values;

namespace Datalake.Database.Tests.Scenarios
{
	public static class TagValues
	{
		static string DbName = "values";

		public static async Task T021_WriteToTag()
		{
			// setup
			using var db = await Setup.CreateDbContextAsync(DbName);
			var admin = await db.GetDefaultAdminAsync();
			var now = DateFormats.GetCurrentDateTime();

			var tag = await db.TagsRepository.CreateAsync(admin, new TagCreateRequest
			{
				Name = "TestTag",
				TagType = TagType.String,
				Frequency = TagFrequency.NotSet,
			});

			// write value
			var response = await db.ValuesRepository.WriteValuesAsSystemAsync(
			[
				new ValueWriteRequest()
				{
					Date = now,
					Guid = Constants.TagGuid,
					Quality = TagQuality.Good,
					Value = Constants.LastValue,
				}
			]);
		}

		public static async Task T3_1_SeedValues()
		{
			using var db = await Setup.CreateDbContextAsync(DbName);

			var now = DateFormats.GetCurrentDateTime();

			ValueWriteRequest[] seedResponse =
			[
				new()
				{
					Guid = Constants.TagGuid,
					Date = Constants.BeforeLastWriteDate,
					Value = Constants.BeforeLastWriteValue,
				},
				new()
				{
					Guid = Constants.TagGuid,
					Date = Constants.FirstWriteDate,
					Value = Constants.FirstWriteValue,
				},
			];

			await db.ValuesRepository.WriteValuesAsSystemAsync(seedResponse);

			seedResponse =
			[
				new()
				{
					Guid = Constants.TagGuid,
					Date = Constants.AfterFirstWriteDate,
					Value = Constants.AfterFirstWriteValue,
				},
			];

			await db.ValuesRepository.WriteValuesAsSystemAsync(seedResponse);

			/*var value = await db.ValuesRepository.GetValuesAsync([
				new ValuesRequest()
				{
					RequestKey = "1",
					Tags = [Constants.TagGuid]
				}
			]);

			Assert.Equal(Constants.LastValue, value[0].Tags[0].Values[0].Value);*/
		}
	}
}
