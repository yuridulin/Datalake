using DatalakeApiClasses.Models.Values;
using DatalakeDatabase.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeApp.ApiControllers;

[ApiController]
[Route("api/Tags/[controller]")]
public class ValuesController(ValuesRepository valuesRepository) : ControllerBase
{
	public const string LiveUrl = "api/Tags/values/live";

	[HttpPost]
	public async Task<List<ValuesResponse>> GetAsync(
		[BindRequired, FromBody] ValuesRequest[] requests)
	{
		var responses = await valuesRepository.GetValuesAsync(requests);

		return responses;
	}

	[HttpPut]
	public async Task<List<ValuesResponse>> WriteAsync(
		[BindRequired, FromBody] ValueWriteRequest[] requests)
	{
		var responses = await valuesRepository.WriteValuesAsync(requests);

		return responses;
	}
}
