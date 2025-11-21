using Datalake.Domain.ValueObjects;

namespace Datalake.Shared.Application.Interfaces.AccessRules;

public interface IUserAccessValuesRepository
{
	Task<Dictionary<Guid, UserAccessValue>> GetAllAsync(
		CancellationToken ct = default);

	Task<UserAccessValue?> GetByUserGuidAsync(
		Guid userGuid, CancellationToken ct = default);

	Task<Dictionary<Guid, UserAccessValue>> GetMultipleAsync(
		IEnumerable<Guid> userGuids,
		CancellationToken ct = default);
}
