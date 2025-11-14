using Datalake.Data.Application.Interfaces;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Attributes;

namespace Datalake.Data.Host.Services;

[Singleton]
public class InventoryApiClient(
	ILogger<InventoryApiClient> logger) : IInventoryApiClient
{
	public async Task<Dictionary<Guid, UserAccessValue>> GetCalculatedAccessAsync(
		IEnumerable<Guid> guids,
		CancellationToken ct = default)
	{
		throw new NotImplementedException("Не сделано");
	}
}
