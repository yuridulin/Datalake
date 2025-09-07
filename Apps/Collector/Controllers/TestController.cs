using Microsoft.AspNetCore.Mvc;

namespace Datalake.Collector.Controllers;

[ApiController]
[Route("api/test")]
public class TestController(
	ILogger<TestController> logger) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<TestResult>> GetAsync()
	{
		logger.LogInformation("Call {method}", nameof(GetAsync));

		await Task.Delay(100);

		var result = new TestResult
		{
			Time = DateTime.UtcNow.Ticks,
		};

		return result;
	}
}

public class TestResult
{
	public double Time { get; set; }
}
