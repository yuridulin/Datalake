using Datalake.Domain.ValueObjects;

namespace Datalake.Inventory.Application.Repositories;

public interface ICalculatedAccessRulesRepository
{
	Task RemoveByBlockId(int value, CancellationToken ct = default);

	Task RemoveBySourceId(int value, CancellationToken ct = default);

	Task RemoveByTagId(int value, CancellationToken ct = default);

	Task RemoveByUserGroupGuid(Guid value, CancellationToken ct = default);

	Task RemoveByUserGuid(Guid value, CancellationToken ct = default);

	Task UpdateAsync(IEnumerable<CalculatedAccessRule> newRules, CancellationToken ct = default);
}
