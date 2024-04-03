using DatalakeDatabase.ApiModels.History;
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
		public async Task<ValuesResponse> WriteValuesAsync(
			[FromBody] ValueWriteRequest request)
		{
		}
	}
}
