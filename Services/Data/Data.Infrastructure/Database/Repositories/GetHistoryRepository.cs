namespace Datalake.Data.Infrastructure.Database.Repositories;
/*
[Scoped]
public class GetHistoryRepository(DataLinqToDbContext db) : IGetHistoryRepository
{
	public async Task<IEnumerable<TagHistory>> GetLastValuesAsync()
	{
		const string sql = $@"
			SELECT DISTINCT ON (""{DataDefinition.TagHistory.TagId}"")
				""{DataDefinition.TagHistory.TagId}"",
				""{DataDefinition.TagHistory.Date}"",
				""{DataDefinition.TagHistory.Text}"",
				""{DataDefinition.TagHistory.Number}"",
				""{DataDefinition.TagHistory.Quality}""
			FROM {DataDefinition.Schema}.""{DataDefinition.TagHistory.Table}""
			ORDER BY ""{DataDefinition.TagHistory.TagId}"", ""{DataDefinition.TagHistory.Date}"" DESC;";

		var values = await db.QueryToArrayAsync<TagHistory>(sql);

		return values;
	}

	public async Task<IEnumerable<TagHistory>> GetLastValuesAsync(int[] tagsId)
	{
		if (tagsId.Length == 0)
			return [];

		const string sql = $@"
			SELECT DISTINCT ON (""{DataDefinition.TagHistory.TagId}"")
				""{DataDefinition.TagHistory.TagId}"",
				""{DataDefinition.TagHistory.Date}"",
				""{DataDefinition.TagHistory.Text}"",
				""{DataDefinition.TagHistory.Number}"",
				""{DataDefinition.TagHistory.Quality}""
			FROM {DataDefinition.Schema}.""{DataDefinition.TagHistory.Table}""
			WHERE ""{DataDefinition.TagHistory.TagId}"" IN {TagsParam}
			ORDER BY ""{DataDefinition.TagHistory.TagId}"", ""{DataDefinition.TagHistory.Date}"" DESC;";

		DataParameter[] parameters = [
			new(TagsParam, tagsId),
		];

		var values = await db.QueryToArrayAsync<TagHistory>(sql, parameters);

		return values;
	}

	public async Task<IEnumerable<TagHistory>> GetExactValuesAsync(DateTime exactDate, int[] tagsId)
	{
		if (tagsId.Length == 0)
			return [];

		const string sql = $@"
			SELECT DISTINCT ON (""{DataDefinition.TagHistory.TagId}"")
				""{DataDefinition.TagHistory.TagId}"",
				""{DataDefinition.TagHistory.Date}"",
				""{DataDefinition.TagHistory.Text}"",
				""{DataDefinition.TagHistory.Number}"",
				""{DataDefinition.TagHistory.Quality}""
			FROM {DataDefinition.Schema}.""{DataDefinition.TagHistory.Table}""
			WHERE
				""{DataDefinition.TagHistory.TagId}"" IN {TagsParam}
				AND ""{DataDefinition.TagHistory.Date}"" <= {ExactParam}
			ORDER BY ""{DataDefinition.TagHistory.TagId}"", ""{DataDefinition.TagHistory.Date}"" DESC";

		DataParameter[] parameters = [
			new(TagsParam, tagsId),
			new(ExactParam, exactDate),
		];

		var values = await db.QueryToArrayAsync<TagHistory>(sql, parameters);

		return values;
	}

	public async Task<IEnumerable<TagHistory>> GetRangeValuesAsync(DateTime fromDate, DateTime toDate, int[] tagsId)
	{
		if (tagsId.Length == 0)
			return [];

		const string sql = $@"
			SELECT 
				""{DataDefinition.TagHistory.TagId}"",
				""{DataDefinition.TagHistory.Date}"",
				""{DataDefinition.TagHistory.Text}"",
				""{DataDefinition.TagHistory.Number}"",
				""{DataDefinition.TagHistory.Quality}""
			FROM (
				SELECT DISTINCT ON (""{DataDefinition.TagHistory.TagId}"")
					""{DataDefinition.TagHistory.TagId}"",
					""{DataDefinition.TagHistory.Date}"",
					""{DataDefinition.TagHistory.Text}"",
					""{DataDefinition.TagHistory.Number}"",
					""{DataDefinition.TagHistory.Quality}""
				FROM {DataDefinition.Schema}.""{DataDefinition.TagHistory.Table}""
				WHERE
					""{DataDefinition.TagHistory.TagId}"" IN {TagsParam}
					AND ""{DataDefinition.TagHistory.Date}"" <= {FromParam}
				ORDER BY ""{DataDefinition.TagHistory.TagId}"", ""{DataDefinition.TagHistory.Date}"" DESC
			) AS valuesBefore
			UNION ALL
			SELECT 
				""{DataDefinition.TagHistory.TagId}"",
				""{DataDefinition.TagHistory.Date}"",
				""{DataDefinition.TagHistory.Text}"",
				""{DataDefinition.TagHistory.Number}"",
				""{DataDefinition.TagHistory.Quality}""
			FROM {DataDefinition.Schema}.""{DataDefinition.TagHistory.Table}""
			WHERE
				""{DataDefinition.TagHistory.TagId}"" IN {TagsParam}
				AND ""{DataDefinition.TagHistory.Date}"" > {FromParam}
				AND ""{DataDefinition.TagHistory.Date}"" <= {ToParam};";

		DataParameter[] parameters = [
			new(TagsParam, tagsId),
			new(FromParam, fromDate),
			new(ToParam, toDate),
		];

		var values = await db.QueryToArrayAsync<TagHistory>(sql, parameters);

		return values;
	}

	const string TagsParam = "@tagsId";
	const string ExactParam = "@exactDate";
	const string FromParam = "@fromDate";
	const string ToParam = "@toDate";
}*/
