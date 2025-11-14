using Datalake.Data.Application.Interfaces;
using Datalake.Domain.ValueObjects;

namespace Datalake.Data.Host.Services;

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
