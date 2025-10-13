using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Repositories;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

public class CalculatedAccessRulesRepository(InventoryDbContext context) : ICalculatedAccessRulesRepository
{
	private static string UpdateSql { get; } = @$"
		INSERT INTO public.""CalculatedAccessRules"" (
			""UserGuid"",
			""AccessType"",
			""IsGlobal"",
			""TagId"",
			""BlockId"",
			""SourceId"",
			""UserGroupGuid"",
			""RuleId"",
			""UpdatedAt""
		)
		VALUES
		ON CONFLICT (""UserGuid"", ""IsGlobal"", ""BlockId"", ""TagId"", ""SourceId"", ""UserGroupGuid"")
		DO UPDATE SET
			""AccessType"" = EXCLUDED.""AccessType"",
			""RuleId"" = EXCLUDED.""RuleId"",
			""UpdatedAt"" = CASE
				WHEN public.""CalculatedAccessRules"".""AccessType"" IS DISTINCT FROM EXCLUDED.""AccessType""
					OR public.""CalculatedAccessRules"".""RuleId"" IS DISTINCT FROM EXCLUDED.""RuleId""
				THEN NOW()
				ELSE public.""CalculatedAccessRules"".""UpdatedAt""
			END
		WHERE
			public.""CalculatedAccessRules"".""AccessType"" IS DISTINCT FROM EXCLUDED.""AccessType""
			OR public.""CalculatedAccessRules"".""RuleId"" IS DISTINCT FROM EXCLUDED.""RuleId"";";

	public Task RemoveByBlockId(int value, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}

	public Task RemoveBySourceId(int value, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}

	public Task RemoveByTagId(int value, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}

	public Task RemoveByUserGroupGuid(Guid value, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}

	public Task RemoveByUserGuid(Guid value, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}

	public async Task UpdateAsync(IEnumerable<CalculatedAccessRule> newRules, CancellationToken ct = default)
	{
		var rulesList = newRules.ToList();
		if (rulesList.Count == 0)
			return;

		// Разбиваем на пачки для избежания проблем с большим количеством параметров
		var batchSize = 100;
		for (int i = 0; i < rulesList.Count; i += batchSize)
		{
			var batch = rulesList.Skip(i).Take(batchSize).ToList();
			await ExecuteUpsertBatchAsync(batch);
		}
	}
}
