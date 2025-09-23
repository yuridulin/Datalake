using Microsoft.AspNetCore.Mvc;

namespace Datalake.Gateway.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExampleController(
	ILogger<ExampleController> logger) : ControllerBase
{
	[HttpGet(Name = "GetWeatherForecast")]
	public async Task<ActionResult<bool>> Get()
	{
		logger.LogInformation("Call {method}", nameof(Get));

		var result = await Task.Run(() => true);

		return result;
	}
}
