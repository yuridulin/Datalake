using DatalakeDatabase.ApiModels.Values;
using DatalakeDatabase.Repositories;

namespace DatalakeDatabase.Tests.Steps
{
	public static class Step003_ManualWriteToTag
	{
		public static async Task WriteToTag()
		{
			using var db = Setup.CreateDbContext();

			var valuesRepository = new ValuesRepository(db);
			var now = DateTime.UtcNow;

			var response = await valuesRepository.WriteValuesAsync(
			[
				new ValueWriteRequest()
				{
					Date = now,
					TagId = Constants.TagId,
					TagQuality = Enums.TagQuality.Good,
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

		public static async Task SeedValues()
		{
			using var db = Setup.CreateDbContext();

			var valuesRepository = new ValuesRepository(db);
			var now = DateTime.UtcNow;

			ValueWriteRequest[] seedResponse =
			[
				new()
				{
					TagId = Constants.TagId,
					Date = Constants.BeforeLastWriteDate,
					Value = Constants.BeforeLastWriteDate,
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
