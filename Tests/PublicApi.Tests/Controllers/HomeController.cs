using Datalake.PublicApiClient;

namespace PublicApi.Tests.Controllers;

public class HomeController(
	ILogger<HomeController> logger)
	: DatalakePublicApiClient(logger, "http://localhost:8010/", "RW1Ayk1YWhrmHOVbm4/9ag3wPrArrPNI6ke6u4Ogppk=")
{
	protected override void ProcessRequest(HttpRequestMessage request)
	{
	}
}
