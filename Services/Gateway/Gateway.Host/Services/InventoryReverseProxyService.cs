using Datalake.Shared.Application.Attributes;

namespace Datalake.Gateway.Host.Services;

/// <summary>
/// Сервис перенаправления запросов к сервису Inventory
/// </summary>
[Singleton]
public class InventoryReverseProxyService(IHttpClientFactory httpClientFactory)
	: ReverseProxyService(httpClientFactory.CreateClient("Inventory"))
{
}
