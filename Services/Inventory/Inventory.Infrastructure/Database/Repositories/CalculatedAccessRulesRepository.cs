using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Infrastructure.Schema;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Text;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

public class CalculatedAccessRulesRepository(InventoryDbContext context) : ICalculatedAccessRulesRepository
{
	public async Task UpdateAsync(IEnumerable<CalculatedAccessRule> newRules, CancellationToken ct = default)
	{
		var rulesList = newRules.ToList();
		if (rulesList.Count == 0)
			return;

		// Разбиваем на пачки для избежания проблем с большим количеством параметров
		var batchSize = 1000; // Увеличиваем размер пачки
		for (int i = 0; i < rulesList.Count; i += batchSize)
		{
			var batch = rulesList.Skip(i).Take(batchSize).ToList();
			await ExecuteBulkUpsertAsync(batch, ct);
		}
	}

	private async Task ExecuteBulkUpsertAsync(List<CalculatedAccessRule> batch, CancellationToken ct)
	{
		// Создаем временную таблицу
		var createTempTableSql = @"
			CREATE TEMP TABLE temp_calculated_access_rules (
					""UserGuid"" uuid NOT NULL,
					""AccessType"" smallint NOT NULL,
					""IsGlobal"" boolean NOT NULL,
					""TagId"" integer NULL,
					""BlockId"" integer NULL,
					""SourceId"" integer NULL,
					""UserGroupGuid"" uuid NULL,
					""RuleId"" integer NOT NULL,
					""UpdatedAt"" timestamptz NOT NULL
			) ON COMMIT DROP;";

		await context.Database.ExecuteSqlRawAsync(createTempTableSql, ct);

		// Вставляем данные во временную таблицу
		await InsertIntoTempTableAsync(batch, ct);

		// Выполняем массовый UPSERT
		var upsertSql = @$"
			-- Обновляем существующие записи
			UPDATE {InventorySchema.Name}.""{InventorySchema.CalculatedAccessRules.Name}"" AS target
			SET 
					""{InventorySchema.CalculatedAccessRules.Columns.AccessType}"" = source.""AccessType"",
					""{InventorySchema.CalculatedAccessRules.Columns.RuleId}"" = source.""RuleId"",
					""{InventorySchema.CalculatedAccessRules.Columns.UpdatedAt}"" = CASE 
							WHEN target.""{InventorySchema.CalculatedAccessRules.Columns.AccessType}"" IS DISTINCT FROM source.""AccessType""
									OR target.""{InventorySchema.CalculatedAccessRules.Columns.RuleId}"" IS DISTINCT FROM source.""RuleId""
							THEN NOW()
							ELSE target.""{InventorySchema.CalculatedAccessRules.Columns.UpdatedAt}""
					END
			FROM temp_calculated_access_rules AS source
			WHERE 
					target.""{InventorySchema.CalculatedAccessRules.Columns.UserGuid}"" = source.""UserGuid""
					AND target.""{InventorySchema.CalculatedAccessRules.Columns.IsGlobal}"" = source.""IsGlobal""
					AND (target.""{InventorySchema.CalculatedAccessRules.Columns.BlockId}"" IS NOT DISTINCT FROM source.""BlockId"")
					AND (target.""{InventorySchema.CalculatedAccessRules.Columns.TagId}"" IS NOT DISTINCT FROM source.""TagId"")
					AND (target.""{InventorySchema.CalculatedAccessRules.Columns.SourceId}"" IS NOT DISTINCT FROM source.""SourceId"")
					AND (target.""{InventorySchema.CalculatedAccessRules.Columns.UserGroupGuid}"" IS NOT DISTINCT FROM source.""UserGroupGuid"");

			-- Вставляем новые записи
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
			SELECT 
					source.""UserGuid"",
					source.""AccessType"",
					source.""IsGlobal"",
					source.""TagId"",
					source.""BlockId"",
					source.""SourceId"",
					source.""UserGroupGuid"",
					source.""RuleId"",
					source.""UpdatedAt""
			FROM temp_calculated_access_rules AS source
			WHERE NOT EXISTS (
					SELECT 1 
					FROM {InventorySchema.Name}.""{InventorySchema.CalculatedAccessRules.Name}"" AS target
					WHERE 
							target.""{InventorySchema.CalculatedAccessRules.Columns.UserGuid}"" = source.""UserGuid""
							AND target.""{InventorySchema.CalculatedAccessRules.Columns.IsGlobal}"" = source.""IsGlobal""
							AND (target.""{InventorySchema.CalculatedAccessRules.Columns.BlockId}"" IS NOT DISTINCT FROM source.""BlockId"")
							AND (target.""{InventorySchema.CalculatedAccessRules.Columns.TagId}"" IS NOT DISTINCT FROM source.""TagId"")
							AND (target.""{InventorySchema.CalculatedAccessRules.Columns.SourceId}"" IS NOT DISTINCT FROM source.""SourceId"")
							AND (target.""{InventorySchema.CalculatedAccessRules.Columns.UserGroupGuid}"" IS NOT DISTINCT FROM source.""UserGroupGuid"")
			);";

		await context.Database.ExecuteSqlRawAsync(upsertSql, ct);
	}

	private async Task InsertIntoTempTableAsync(List<CalculatedAccessRule> batch, CancellationToken ct)
	{
		var sqlBuilder = new StringBuilder();
		var parameters = new List<NpgsqlParameter>();

		sqlBuilder.AppendLine(@"
      INSERT INTO temp_calculated_access_rules (
        ""UserGuid"", ""AccessType"", ""IsGlobal"", ""TagId"", ""BlockId"", ""SourceId"", ""UserGroupGuid"", ""RuleId"", ""UpdatedAt""
      ) VALUES ");

		for (int i = 0; i < batch.Count; i++)
		{
			var r = batch[i];
			if (i > 0)
				sqlBuilder.Append(", ");

			sqlBuilder.Append($@"(@p{i}_userGuid, @p{i}_accessType, @p{i}_isGlobal, @p{i}_tagId, @p{i}_blockId, @p{i}_sourceId, @p{i}_userGroupGuid, @p{i}_ruleId, @p{i}_updatedAt)");

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

		await context.Database.ExecuteSqlRawAsync(sqlBuilder.ToString(), parameters.ToArray(), ct);
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
