using Datalake.Shared.Application.Attributes;

namespace Datalake.Gateway.Host.Services;

[Singleton]
public class InventoryReverseProxyService(IHttpClientFactory httpClientFactory)
	: ReverseProxyService(httpClientFactory.CreateClient("Inventory"))
{
}
