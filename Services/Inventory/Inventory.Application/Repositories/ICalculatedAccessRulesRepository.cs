using Datalake.Domain.ValueObjects;

namespace Datalake.Inventory.Application.Repositories;

public interface ICalculatedAccessRulesRepository
{
	Task UpdateAsync(IEnumerable<CalculatedAccessRule> newRules, CancellationToken ct = default);
}
