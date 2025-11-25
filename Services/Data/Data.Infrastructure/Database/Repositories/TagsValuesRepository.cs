using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure;
using Datalake.Shared.Infrastructure.Database.Schema;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.Database.Repositories;

[Scoped]
public class TagsValuesRepository(
	DataDbLinqContext db,
	ILogger<TagsValuesRepository> logger) : ITagsValuesRepository
{
	public async Task<bool> WriteAsync(IReadOnlyList<TagValue> batch)
	{
		return await Measures.MeasureAsync(async () =>
		{
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				await db.ExecuteAsync(CreateTempTableForWrite);
				await db.BulkCopyAsync(BulkCopyOptions, batch);
				await db.ExecuteAsync(WriteSql);

				await transaction.CommitAsync();

				logger.LogDebug("Записано значений: {count}", batch.Count);
				return true;
			}
			catch (Exception e)
			{
				logger.LogError(e, "Не удалось записать данные");
				await transaction.RollbackAsync();
				return false;
			}
		}, logger, nameof(WriteAsync));
	}

	public async Task<IEnumerable<TagValue>> GetAllLastAsync()
	{
		var values = await db.QueryToArrayAsync<TagValue>(GetAllLastSql);

		return values;
	}

	public async Task<IEnumerable<TagValue>> GetLastAsync(IEnumerable<int> tagsIdentifiers)
	{
		if (!tagsIdentifiers.Any())
			return [];

		DataParameter[] parameters = [
			new(TagsParam, tagsIdentifiers),
		];

		var values = await db.QueryToArrayAsync<TagValue>(GetLastSql, parameters);

		return values;
	}

	public async Task<IEnumerable<TagValue>> GetExactAsync(IEnumerable<int> tagsIdentifiers, DateTime exactDate)
	{
		if (!tagsIdentifiers.Any())
			return [];

		DataParameter[] parameters = [
			new(TagsParam, tagsIdentifiers),
			new(ExactParam, exactDate),
		];

		var values = await db.QueryToArrayAsync<TagValue>(GetExactSql, parameters);

		return values;
	}

	public async Task<IEnumerable<TagValue>> GetRangeAsync(IEnumerable<int> tagsIdentifiers, DateTime from, DateTime to)
	{
		if (!tagsIdentifiers.Any())
			return [];

		DataParameter[] parameters = [
			new(TagsParam, tagsIdentifiers),
			new(FromParam, from),
			new(ToParam, to),
		];

		var values = await db.QueryToArrayAsync<TagValue>(GetRangeSql, parameters);

		return values;
	}


	private static string TagsParam { get; } = "@tagsId";

	private static string ExactParam { get; } = "@exactDate";

	private static string FromParam { get; } = "@fromDate";

	private static string ToParam { get; } = "@toDate";

	private static string TempTableForWrite { get; } = "TagsValuesTemp";

	private static BulkCopyOptions BulkCopyOptions { get; } = new()
	{
		TableOptions = TableOptions.IsTemporary,
		TableName = TempTableForWrite,
		BulkCopyType = BulkCopyType.ProviderSpecific,
	};

	private static string CreateTempTableForWrite { get; } = $@"
		CREATE TEMPORARY TABLE ""{TempTableForWrite}"" (LIKE {DataSchema.Name}.""{DataSchema.TagsValues.Name}"" EXCLUDING INDEXES)
		ON COMMIT DROP;";

	private static string WriteSql { get; } = $@"
		INSERT INTO {DataSchema.Name}.""{DataSchema.TagsValues.Name}"" (
			""{DataSchema.TagsValues.Columns.TagId}"",
			""{DataSchema.TagsValues.Columns.Date}"",
			""{DataSchema.TagsValues.Columns.Text}"",
			""{DataSchema.TagsValues.Columns.Number}"",
			""{DataSchema.TagsValues.Columns.Quality}""
		)
		SELECT
			""{DataSchema.TagsValues.Columns.TagId}"",
			""{DataSchema.TagsValues.Columns.Date}"",
			""{DataSchema.TagsValues.Columns.Text}"",
			""{DataSchema.TagsValues.Columns.Number}"",
			""{DataSchema.TagsValues.Columns.Quality}""
		FROM ""{TempTableForWrite}""
		ON CONFLICT (""{DataSchema.TagsValues.Columns.TagId}"", ""{DataSchema.TagsValues.Columns.Date}"")
		DO UPDATE SET
			""{DataSchema.TagsValues.Columns.Text}""    = EXCLUDED.""{DataSchema.TagsValues.Columns.Text}"",
			""{DataSchema.TagsValues.Columns.Number}""  = EXCLUDED.""{DataSchema.TagsValues.Columns.Number}"",
			""{DataSchema.TagsValues.Columns.Quality}"" = EXCLUDED.""{DataSchema.TagsValues.Columns.Quality}"";";

	private static string GetAllLastSql { get; } = $@"
		SELECT DISTINCT ON (""{DataSchema.TagsValues.Columns.TagId}"")
			""{DataSchema.TagsValues.Columns.TagId}"",
			""{DataSchema.TagsValues.Columns.Date}"",
			""{DataSchema.TagsValues.Columns.Text}"",
			""{DataSchema.TagsValues.Columns.Number}"",
			""{DataSchema.TagsValues.Columns.Quality}""
		FROM {DataSchema.Name}.""{DataSchema.TagsValues.Name}""
		ORDER BY ""{DataSchema.TagsValues.Columns.TagId}"", ""{DataSchema.TagsValues.Columns.Date}"" DESC;";

	private static string GetLastSql { get; } = $@"
		SELECT DISTINCT ON (""{DataSchema.TagsValues.Columns.TagId}"")
			""{DataSchema.TagsValues.Columns.TagId}"",
			""{DataSchema.TagsValues.Columns.Date}"",
			""{DataSchema.TagsValues.Columns.Text}"",
			""{DataSchema.TagsValues.Columns.Number}"",
			""{DataSchema.TagsValues.Columns.Quality}""
		FROM {DataSchema.Name}.""{DataSchema.TagsValues.Name}""
		WHERE ""{DataSchema.TagsValues.Columns.TagId}"" IN {TagsParam}
		ORDER BY ""{DataSchema.TagsValues.Columns.TagId}"", ""{DataSchema.TagsValues.Columns.Date}"" DESC;";

	private static string GetExactSql { get; } = $@"
		SELECT DISTINCT ON (""{DataSchema.TagsValues.Columns.TagId}"")
			""{DataSchema.TagsValues.Columns.TagId}"",
			""{DataSchema.TagsValues.Columns.Date}"",
			""{DataSchema.TagsValues.Columns.Text}"",
			""{DataSchema.TagsValues.Columns.Number}"",
			""{DataSchema.TagsValues.Columns.Quality}""
		FROM {DataSchema.Name}.""{DataSchema.TagsValues.Name}""
		WHERE
			""{DataSchema.TagsValues.Columns.TagId}"" = ANY({TagsParam})
			AND ""{DataSchema.TagsValues.Columns.Date}"" <= {ExactParam}
		ORDER BY ""{DataSchema.TagsValues.Columns.TagId}"", ""{DataSchema.TagsValues.Columns.Date}"" DESC";

	private static string GetRangeSql { get; } = $@"
		SELECT 
			""{DataSchema.TagsValues.Columns.TagId}"",
			""{DataSchema.TagsValues.Columns.Date}"",
			""{DataSchema.TagsValues.Columns.Text}"",
			""{DataSchema.TagsValues.Columns.Number}"",
			""{DataSchema.TagsValues.Columns.Quality}""
		FROM (
			SELECT DISTINCT ON (""{DataSchema.TagsValues.Columns.TagId}"")
				""{DataSchema.TagsValues.Columns.TagId}"",
				""{DataSchema.TagsValues.Columns.Date}"",
				""{DataSchema.TagsValues.Columns.Text}"",
				""{DataSchema.TagsValues.Columns.Number}"",
				""{DataSchema.TagsValues.Columns.Quality}""
			FROM {DataSchema.Name}.""{DataSchema.TagsValues.Name}""
			WHERE
				""{DataSchema.TagsValues.Columns.TagId}"" IN {TagsParam}
				AND ""{DataSchema.TagsValues.Columns.Date}"" <= {FromParam}
			ORDER BY ""{DataSchema.TagsValues.Columns.TagId}"", ""{DataSchema.TagsValues.Columns.Date}"" DESC
		) AS valuesBefore
		UNION ALL
		SELECT 
			""{DataSchema.TagsValues.Columns.TagId}"",
			""{DataSchema.TagsValues.Columns.Date}"",
			""{DataSchema.TagsValues.Columns.Text}"",
			""{DataSchema.TagsValues.Columns.Number}"",
			""{DataSchema.TagsValues.Columns.Quality}""
		FROM {DataSchema.Name}.""{DataSchema.TagsValues.Name}""
		WHERE
			""{DataSchema.TagsValues.Columns.TagId}"" IN {TagsParam}
			AND ""{DataSchema.TagsValues.Columns.Date}"" > {FromParam}
			AND ""{DataSchema.TagsValues.Columns.Date}"" <= {ToParam};";
}
