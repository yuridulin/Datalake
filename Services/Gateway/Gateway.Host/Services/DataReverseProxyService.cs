using Datalake.Shared.Application.Attributes;

namespace Datalake.Gateway.Host.Services;

[Singleton]
public class DataReverseProxyService(IHttpClientFactory httpClientFactory)
	: ReverseProxyService(httpClientFactory.CreateClient("Data"))
{
}
