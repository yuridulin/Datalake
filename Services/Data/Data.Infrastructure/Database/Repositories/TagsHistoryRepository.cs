using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure.Schema;
using LinqToDB.Data;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.Database.Repositories;

[Scoped]
public class TagsHistoryRepository(
	DataLinqToDbContext db,
	ILogger<TagsHistoryRepository> logger) : ITagsHistoryRepository
{
	public async Task WriteAsync(IEnumerable<TagHistory> batch)
	{
		using var transaction = await db.BeginTransactionAsync();

		try
		{
			await db.ExecuteAsync(CreateTempTableForWrite);
			await db.BulkCopyAsync(BulkCopyOptions, batch);
			await db.ExecuteAsync(WriteSql);

			await transaction.CommitAsync();
		}
		catch (Exception e)
		{
			logger.LogError(e, "Не удалось записать данные");
			await transaction.RollbackAsync();
			throw;
		}
	}

	public async Task<IEnumerable<TagHistory>> GetAllLastAsync()
	{
		var values = await db.QueryToArrayAsync<TagHistory>(GetAllLastSql);

		return values;
	}

	public async Task<IEnumerable<TagHistory>> GetLastAsync(IEnumerable<int> tagsIdentifiers)
	{
		if (!tagsIdentifiers.Any())
			return [];

		DataParameter[] parameters = [
			new(TagsParam, tagsIdentifiers),
		];

		var values = await db.QueryToArrayAsync<TagHistory>(GetLastSql, parameters);

		return values;
	}

	public async Task<IEnumerable<TagHistory>> GetExactAsync(IEnumerable<int> tagsIdentifiers, DateTime exactDate)
	{
		if (!tagsIdentifiers.Any())
			return [];

		DataParameter[] parameters = [
			new(TagsParam, tagsIdentifiers),
			new(ExactParam, exactDate),
		];

		var values = await db.QueryToArrayAsync<TagHistory>(GetExactSql, parameters);

		return values;
	}

	public async Task<IEnumerable<TagHistory>> GetRangeAsync(IEnumerable<int> tagsIdentifiers, DateTime from, DateTime to)
	{
		if (!tagsIdentifiers.Any())
			return [];

		DataParameter[] parameters = [
			new(TagsParam, tagsIdentifiers),
			new(FromParam, from),
			new(ToParam, to),
		];

		var values = await db.QueryToArrayAsync<TagHistory>(GetRangeSql, parameters);

		return values;
	}


	private static string TagsParam { get; } = "@tagsId";

	private static string ExactParam { get; } = "@exactDate";

	private static string FromParam { get; } = "@fromDate";

	private static string ToParam { get; } = "@toDate";

	private static string TempTableForWrite { get; } = "TagsHistoryState";

	private static BulkCopyOptions BulkCopyOptions { get; } = new() { TableName = TempTableForWrite, BulkCopyType = BulkCopyType.ProviderSpecific, };

	private static string CreateTempTableForWrite { get; } = $@"
		CREATE TEMPORARY TABLE ""{TempTableForWrite}"" (LIKE {DataSchema.Name}.""{DataSchema.TagsHistory.Name}"" EXCLUDING INDEXES)
		ON COMMIT DROP;";

	private static string WriteSql { get; } = $@"
		INSERT INTO {DataSchema.Name}.""{DataSchema.TagsHistory.Name}"" (
			""{DataSchema.TagsHistory.Columns.TagId}"",
			""{DataSchema.TagsHistory.Columns.Date}"",
			""{DataSchema.TagsHistory.Columns.Text}"",
			""{DataSchema.TagsHistory.Columns.Number}"",
			""{DataSchema.TagsHistory.Columns.Quality}""
		)
		SELECT
			""{DataSchema.TagsHistory.Columns.TagId}"",
			""{DataSchema.TagsHistory.Columns.Date}"",
			""{DataSchema.TagsHistory.Columns.Text}"",
			""{DataSchema.TagsHistory.Columns.Number}"",
			""{DataSchema.TagsHistory.Columns.Quality}""
		FROM ""{TempTableForWrite}""
		ON CONFLICT (""{DataSchema.TagsHistory.Columns.TagId}"", ""{DataSchema.TagsHistory.Columns.Date}"")
		DO UPDATE SET
			""{DataSchema.TagsHistory.Columns.Text}""    = EXCLUDED.""{DataSchema.TagsHistory.Columns.Text}"",
			""{DataSchema.TagsHistory.Columns.Number}""  = EXCLUDED.""{DataSchema.TagsHistory.Columns.Number}"",
			""{DataSchema.TagsHistory.Columns.Quality}"" = EXCLUDED.""{DataSchema.TagsHistory.Columns.Quality}"";";

	private static string GetAllLastSql { get; } = $@"
		SELECT DISTINCT ON (""{DataSchema.TagsHistory.Columns.TagId}"")
			""{DataSchema.TagsHistory.Columns.TagId}"",
			""{DataSchema.TagsHistory.Columns.Date}"",
			""{DataSchema.TagsHistory.Columns.Text}"",
			""{DataSchema.TagsHistory.Columns.Number}"",
			""{DataSchema.TagsHistory.Columns.Quality}""
		FROM {DataSchema.Name}.""{DataSchema.TagsHistory.Name}""
		ORDER BY ""{DataSchema.TagsHistory.Columns.TagId}"", ""{DataSchema.TagsHistory.Columns.Date}"" DESC;";

	private static string GetLastSql { get; } = $@"
		SELECT DISTINCT ON (""{DataSchema.TagsHistory.Columns.TagId}"")
			""{DataSchema.TagsHistory.Columns.TagId}"",
			""{DataSchema.TagsHistory.Columns.Date}"",
			""{DataSchema.TagsHistory.Columns.Text}"",
			""{DataSchema.TagsHistory.Columns.Number}"",
			""{DataSchema.TagsHistory.Columns.Quality}""
		FROM {DataSchema.Name}.""{DataSchema.TagsHistory.Name}""
		WHERE ""{DataSchema.TagsHistory.Columns.TagId}"" IN {TagsParam}
		ORDER BY ""{DataSchema.TagsHistory.Columns.TagId}"", ""{DataSchema.TagsHistory.Columns.Date}"" DESC;";

	private static string GetExactSql { get; } = $@"
		SELECT DISTINCT ON (""{DataSchema.TagsHistory.Columns.TagId}"")
			""{DataSchema.TagsHistory.Columns.TagId}"",
			""{DataSchema.TagsHistory.Columns.Date}"",
			""{DataSchema.TagsHistory.Columns.Text}"",
			""{DataSchema.TagsHistory.Columns.Number}"",
			""{DataSchema.TagsHistory.Columns.Quality}""
		FROM {DataSchema.Name}.""{DataSchema.TagsHistory.Name}""
		WHERE
			""{DataSchema.TagsHistory.Columns.TagId}"" IN {TagsParam}
			AND ""{DataSchema.TagsHistory.Columns.Date}"" <= {ExactParam}
		ORDER BY ""{DataSchema.TagsHistory.Columns.TagId}"", ""{DataSchema.TagsHistory.Columns.Date}"" DESC";

	private static string GetRangeSql { get; } = $@"
		SELECT 
			""{DataSchema.TagsHistory.Columns.TagId}"",
			""{DataSchema.TagsHistory.Columns.Date}"",
			""{DataSchema.TagsHistory.Columns.Text}"",
			""{DataSchema.TagsHistory.Columns.Number}"",
			""{DataSchema.TagsHistory.Columns.Quality}""
		FROM (
			SELECT DISTINCT ON (""{DataSchema.TagsHistory.Columns.TagId}"")
				""{DataSchema.TagsHistory.Columns.TagId}"",
				""{DataSchema.TagsHistory.Columns.Date}"",
				""{DataSchema.TagsHistory.Columns.Text}"",
				""{DataSchema.TagsHistory.Columns.Number}"",
				""{DataSchema.TagsHistory.Columns.Quality}""
			FROM {DataSchema.Name}.""{DataSchema.TagsHistory.Name}""
			WHERE
				""{DataSchema.TagsHistory.Columns.TagId}"" IN {TagsParam}
				AND ""{DataSchema.TagsHistory.Columns.Date}"" <= {FromParam}
			ORDER BY ""{DataSchema.TagsHistory.Columns.TagId}"", ""{DataSchema.TagsHistory.Columns.Date}"" DESC
		) AS valuesBefore
		UNION ALL
		SELECT 
			""{DataSchema.TagsHistory.Columns.TagId}"",
			""{DataSchema.TagsHistory.Columns.Date}"",
			""{DataSchema.TagsHistory.Columns.Text}"",
			""{DataSchema.TagsHistory.Columns.Number}"",
			""{DataSchema.TagsHistory.Columns.Quality}""
		FROM {DataSchema.Name}.""{DataSchema.TagsHistory.Name}""
		WHERE
			""{DataSchema.TagsHistory.Columns.TagId}"" IN {TagsParam}
			AND ""{DataSchema.TagsHistory.Columns.Date}"" > {FromParam}
			AND ""{DataSchema.TagsHistory.Columns.Date}"" <= {ToParam};";
}
