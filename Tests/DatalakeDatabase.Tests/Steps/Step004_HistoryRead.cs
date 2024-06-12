using DatalakeApiClasses.Models.Values;
using DatalakeDatabase.Repositories;

namespace DatalakeDatabase.Tests.Steps
{
	public static class Step004_HistoryRead
	{
		public static async Task T4_1_ReadExactBetweenFirstAndSecond()
		{
			using var db = Setup.CreateDbContext();

			var valuesRepository = new ValuesRepository(db);

			var response = await valuesRepository.GetValuesAsync([
				new ValuesRequest()
				{
					RequestKey = "1",
					Exact = Constants.InCenterDate,
					Tags = [Constants.TagGuid]
				}
			]);

			Assert.NotNull(response);
			Assert.Single(response);

			var values = response[0].Tags[0].Values;
			Assert.NotNull(values);
			Assert.Single(values);

			var value = values[0];
			Assert.NotNull(value);
			Assert.Equal(Constants.InCenterValue, value.Value);
		}
	}
}
