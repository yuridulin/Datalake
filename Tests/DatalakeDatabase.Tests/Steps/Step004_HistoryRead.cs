using DatalakeDatabase.ApiModels.Values;
using DatalakeDatabase.Repositories;

namespace DatalakeDatabase.Tests.Steps
{
	public static class Step004_HistoryRead
	{
		public static async Task ReadExactBetweenFirstAndSecond()
		{
			using var db = Setup.CreateDbContext();

			var valuesRepository = new ValuesRepository(db);

			var response = await valuesRepository.GetValuesAsync([
				new ValuesRequest()
				{
					Exact = Constants.InCenterDate,
					TagNames = [Constants.TagName]
				}
			]);

			Assert.NotNull(response);
			Assert.Single(response);

			var values = response[0].Values;
			Assert.NotNull(values);
			Assert.Single(values);

			var value = values[0];
			Assert.NotNull(value);
			Assert.Equal(Constants.InCenterValue, value.Value);
		}
	}
}
