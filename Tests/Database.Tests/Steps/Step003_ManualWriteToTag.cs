using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Models.Values;
using DatalakeDatabase.Repositories;

namespace DatalakeDatabase.Tests.Steps
{
	public static class Step003_ManualWriteToTag
	{
		public static async Task T021_WriteToTag()
		{
			using var db = Setup.CreateDbContext();

			var valuesRepository = new ValuesRepository(db);
			var now = DateTime.Now;

			var response = await valuesRepository.WriteValuesAsync(
			[
				new ValueWriteRequest()
				{
					Date = now,
					TagId = Constants.TagId,
					TagQuality = TagQuality.Good,
					Value = Constants.LastValue,
				}
			]);

			Assert.Single(response);
			Assert.True(response[0].Id == Constants.TagId);
			Assert.Single(response[0].Values);
			Assert.True(response[0].Values[0].Date == now);
			Assert.NotNull(response[0].Values[0].Value);
			Assert.Equal(response[0].Values[0].Value, Constants.LastValue);
		}

		public static async Task T3_1_SeedValues()
		{
			DatalakeContext.SetupLinqToDB();
			using var db = Setup.CreateDbContext();

			var valuesRepository = new ValuesRepository(db);
			var now = DateTime.Now;

			ValueWriteRequest[] seedResponse =
			[
				new()
				{
					TagId = Constants.TagId,
					Date = Constants.BeforeLastWriteDate,
					Value = Constants.BeforeLastWriteValue,
				},
				new()
				{
					TagId = Constants.TagId,
					Date = Constants.FirstWriteDate,
					Value = Constants.FirstWriteValue,
				},
			];

			await valuesRepository.WriteValuesAsync(seedResponse);

			seedResponse =
			[
				new()
				{
					TagId = Constants.TagId,
					Date = Constants.AfterFirstWriteDate,
					Value = Constants.AfterFirstWriteValue,
				},
			];

			await valuesRepository.WriteValuesAsync(seedResponse);

			var value = await valuesRepository.GetValuesAsync([ new ValuesRequest() { Tags = [Constants.TagId] } ]);

			Assert.Equal(Constants.LastValue, value[0].Values[0].Value);
		}
	}
}
