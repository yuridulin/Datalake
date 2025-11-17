using Datalake.Domain.ValueObjects;

namespace Datalake.Data.Application.Interfaces;

public interface IInventoryApiClient
{
	Task<Dictionary<Guid, UserAccessValue>> GetCalculatedAccessAsync(
		IEnumerable<Guid> guids,
		CancellationToken cancellationToken = default);
}
