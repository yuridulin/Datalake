using DatalakeDatabase.ApiModels.Values;
using DatalakeDatabase.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeApp.ApiControllers
{
	[ApiController]
	[Route("api/Tags/[controller]")]
	public class ValuesController(ValuesRepository valuesRepository) : ControllerBase
	{
		public const string LiveUrl = "api/Tags/values/live";

		[HttpPost]
		public async Task<List<ValuesResponse>> GetValuesAsync(
			[FromBody] ValuesRequest[] requests)
		{
			var responses = await valuesRepository.GetValuesAsync(requests);

			return responses;
		}

		[HttpPut]
		public async Task<List<ValuesResponse>> WriteValuesAsync(
			[FromBody] ValueWriteRequest[] requests)
		{
			var responses = await valuesRepository.WriteValuesAsync(requests);

			return responses;
		}
	}
}
