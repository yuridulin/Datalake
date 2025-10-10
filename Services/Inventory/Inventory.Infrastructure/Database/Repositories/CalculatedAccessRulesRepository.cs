using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Repositories;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

public class CalculatedAccessRulesRepository(InventoryDbContext context) : ICalculatedAccessRulesRepository
{
	private static string UpdateSql { get; } = @$"";

	public Task UpdateAsync(IEnumerable<CalculatedAccessRule> newRules, CancellationToken ct = default)
	{


		return Task.CompletedTask;
	}
}
