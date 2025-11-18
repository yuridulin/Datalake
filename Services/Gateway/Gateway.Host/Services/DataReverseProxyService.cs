using Datalake.Shared.Application.Attributes;

namespace Datalake.Gateway.Host.Services;

/// <summary>
/// Сервис перенаправления запросов к сервису Data
/// </summary>
[Singleton]
public class DataReverseProxyService(IHttpClientFactory httpClientFactory)
	: ReverseProxyService(httpClientFactory.CreateClient("Data"))
{
}
