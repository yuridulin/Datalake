using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

public class CalculatedAccessRulesRepository(InventoryDbContext context) : ICalculatedAccessRulesRepository
{
	private static string UpsertSql { get; } = @$"
		INSERT INTO {InventorySchema.Name}.""{InventorySchema.CalculatedAccessRules.Name}"" (
			""{InventorySchema.CalculatedAccessRules.Columns.UserGuid}"",
			""{InventorySchema.CalculatedAccessRules.Columns.AccessType}"",
			""{InventorySchema.CalculatedAccessRules.Columns.IsGlobal}"",
			""{InventorySchema.CalculatedAccessRules.Columns.TagId}"",
			""{InventorySchema.CalculatedAccessRules.Columns.BlockId}"",
			""{InventorySchema.CalculatedAccessRules.Columns.SourceId}"",
			""{InventorySchema.CalculatedAccessRules.Columns.UserGroupGuid}"",
			""{InventorySchema.CalculatedAccessRules.Columns.RuleId}"",
			""{InventorySchema.CalculatedAccessRules.Columns.UpdatedAt}""
		)
		VALUES
		ON CONFLICT (
			""{InventorySchema.CalculatedAccessRules.Columns.UserGuid}"",
			""{InventorySchema.CalculatedAccessRules.Columns.IsGlobal}"",
			""{InventorySchema.CalculatedAccessRules.Columns.BlockId}"",
			""{InventorySchema.CalculatedAccessRules.Columns.TagId}"",
			""{InventorySchema.CalculatedAccessRules.Columns.SourceId}"",
			""{InventorySchema.CalculatedAccessRules.Columns.UserGroupGuid}""
		)
		DO UPDATE SET
			""{InventorySchema.CalculatedAccessRules.Columns.AccessType}"" = EXCLUDED.""{InventorySchema.CalculatedAccessRules.Columns.AccessType}"",
			""{InventorySchema.CalculatedAccessRules.Columns.RuleId}"" = EXCLUDED.""{InventorySchema.CalculatedAccessRules.Columns.RuleId}"",
			""{InventorySchema.CalculatedAccessRules.Columns.UpdatedAt}"" = CASE
				WHEN {InventorySchema.Name}.""{InventorySchema.CalculatedAccessRules.Name}"".""{InventorySchema.CalculatedAccessRules.Columns.AccessType}"" IS DISTINCT FROM EXCLUDED.""{InventorySchema.CalculatedAccessRules.Columns.AccessType}""
					OR {InventorySchema.Name}.""{InventorySchema.CalculatedAccessRules.Name}"".""{InventorySchema.CalculatedAccessRules.Columns.RuleId}"" IS DISTINCT FROM EXCLUDED.""{InventorySchema.CalculatedAccessRules.Columns.RuleId}""
				THEN NOW()
				ELSE {InventorySchema.Name}.""{InventorySchema.CalculatedAccessRules.Name}"".""{InventorySchema.CalculatedAccessRules.Columns.UpdatedAt}""
			END
		WHERE
			{InventorySchema.Name}.""{InventorySchema.CalculatedAccessRules.Name}"".""{InventorySchema.CalculatedAccessRules.Columns.AccessType}"" IS DISTINCT FROM EXCLUDED.""{InventorySchema.CalculatedAccessRules.Columns.AccessType}""
			OR {InventorySchema.Name}.""{InventorySchema.CalculatedAccessRules.Name}"".""{InventorySchema.CalculatedAccessRules.Columns.RuleId}"" IS DISTINCT FROM EXCLUDED.""{InventorySchema.CalculatedAccessRules.Columns.RuleId}"";";

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

	private async Task ExecuteUpsertBatchAsync(List<CalculatedAccessRule> batch)
	{
		var parameters = new List<object>();
		var valuesSql = new StringBuilder();

		for (int i = 0; i < batch.Count; i++)
		{
			var r = batch[i];
			if (i > 0)
				valuesSql.Append(", ");

			valuesSql.Append($"(@p{i}_userGuid, @p{i}_accessType, @p{i}_isGlobal, @p{i}_tagId, @p{i}_blockId, @p{i}_sourceId, @p{i}_userGroupGuid, @p{i}_ruleId, @p{i}_updatedAt)");

			parameters.Add(new NpgsqlParameter($"p{i}_userGuid", r.UserGuid));
			parameters.Add(new NpgsqlParameter($"p{i}_accessType", (int)r.AccessType));
			parameters.Add(new NpgsqlParameter($"p{i}_isGlobal", r.IsGlobal));
			parameters.Add(new NpgsqlParameter($"p{i}_tagId", (object?)r.TagId ?? DBNull.Value));
			parameters.Add(new NpgsqlParameter($"p{i}_blockId", (object?)r.BlockId ?? DBNull.Value));
			parameters.Add(new NpgsqlParameter($"p{i}_sourceId", (object?)r.SourceId ?? DBNull.Value));
			parameters.Add(new NpgsqlParameter($"p{i}_userGroupGuid", (object?)r.UserGroupGuid ?? DBNull.Value));
			parameters.Add(new NpgsqlParameter($"p{i}_ruleId", (object?)r.RuleId ?? DBNull.Value));
			parameters.Add(new NpgsqlParameter($"p{i}_updatedAt", r.UpdatedAt));
		}

		var sql = UpsertSql.Replace("VALUES", "VALUES " + valuesSql);

		await context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());
	}

	public async Task RemoveByBlockId(int blockId, CancellationToken ct = default)
	{
		await context.CalculatedAccessRules
			.Where(x => x.BlockId == blockId)
			.ExecuteDeleteAsync(ct);
	}

	public async Task RemoveBySourceId(int sourceId, CancellationToken ct = default)
	{
		await context.CalculatedAccessRules
			.Where(x => x.SourceId == sourceId)
			.ExecuteDeleteAsync(ct);
	}

	public async Task RemoveByTagId(int tagId, CancellationToken ct = default)
	{
		await context.CalculatedAccessRules
			.Where(x => x.TagId == tagId)
			.ExecuteDeleteAsync(ct);
	}

	public async Task RemoveByUserGroupGuid(Guid userGroupGuid, CancellationToken ct = default)
	{
		await context.CalculatedAccessRules
			.Where(x => x.UserGroupGuid == userGroupGuid)
			.ExecuteDeleteAsync(ct);
	}

	public async Task RemoveByUserGuid(Guid userGuid, CancellationToken ct = default)
	{
		await context.CalculatedAccessRules
			.Where(x => x.UserGuid == userGuid)
			.ExecuteDeleteAsync(ct);
	}
}
