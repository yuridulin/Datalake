using Datalake.Database.Enums;
using Datalake.Database.Models.Tables;
using Datalake.Database.Tables;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с партицированными таблицами значений
/// </summary>
public class TablesRepository(DatalakeContext db)
{
	#region Действия

	/// <summary>
	/// Получение списка существующих таблиц значений из БД, включая данные о наличии индекса
	/// </summary>
	/// <returns>Список существующих таблиц</returns>
	public async Task<HistoryTableInfo[]> GetHistoryTablesFromSchema()
	{
		return await PostgreSQL_GetHistoryTablesFromSchema();
	}

	/// <summary>
	/// Создание индекса для таблицы значений по идентификаторам тегов и дате
	/// </summary>
	/// <param name="tableName">Название таблицы</param>
	public async Task CreateHistoryIndex(string tableName)
	{
		await PostgreSQL_CreateHistoryIndex(tableName);

		db.Insert(new Log
		{
			Category = LogCategory.Database,
			Type = LogType.Trace,
			Text = "Создан индекс для партиции: " + tableName,
		});
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="date"></param>
	/// <returns></returns>
	public async Task EnsureInitialValues(DateTime date)
	{
		var table = db.GetTable<TagHistory>().TableName(GetTableName(date));

		bool hasInitial = await table.AnyAsync(x => x.Quality == TagQuality.Bad_LOCF || x.Quality == TagQuality.Good_LOCF);

		if (!hasInitial)
			await WriteInitialValuesAsync(date);
	}

	#endregion

	#region Кэш

	/// <summary>
	/// Кэшированный список существующих таблиц
	/// </summary>
	public static Dictionary<DateTime, string> CachedTables { get; set; } = [];

	#endregion

	#region Манипулирование таблицами

	static object locker = new();

	internal const string NamePrefix = "TagsHistory_";
	internal const string DateMask = "yyyy_MM_dd";
	internal const string IndexPostfix = "_idx";

	internal static string GetTableName(DateTime date) => NamePrefix + date.ToString(DateMask);

	internal static DateTime GetTableDate(string tableName) => DateTime.TryParseExact(
		tableName.AsSpan(NamePrefix.Length),
		DateMask,
		null,
		System.Globalization.DateTimeStyles.None,
		out var d) ? d : DateTime.MinValue;

	internal static DateTime? GetNextTableDate(DateTime date)
	{
		return CachedTables.Keys.Where(x => x > date).OrderBy(x => x).FirstOrDefault();
	}

	internal static DateTime? GetPreviousTableDate(DateTime date)
	{
		return CachedTables.Keys.Where(x => x < date).OrderByDescending(x => x).FirstOrDefault();
	}

	internal ITable<TagHistory> GetHistoryTable(DateTime seekDate)
	{
		DateTime date = seekDate.Date;
		ITable<TagHistory> table;

		if (CachedTables.TryGetValue(date, out string? value))
		{
			table = db.GetTable<TagHistory>().TableName(value);
		}
		else
		{
			var tableName = GetTableName(date);
			table = db.CreateTable<TagHistory>(tableName);

			lock (locker)
			{
				CachedTables.Add(date, tableName);
			}

			WriteInitialValuesAsync(date).Wait();

			db.Insert(new Log
			{
				Category = LogCategory.Database,
				Type = LogType.Trace,
				Text = "Создана партиция: " + tableName,
			});
		}

		return table;
	}

	async Task WriteInitialValuesAsync(DateTime date)
	{
		var table = db.TablesRepository.GetHistoryTable(date);

		// заполнение начальных значений
		DateTime? previous = CachedTables.Keys.Where(x => x < date).OrderByDescending(x => x).FirstOrDefault();
		if (previous != null)
		{
			var previousTable = db.GetTable<TagHistory>().TableName(GetTableName(previous.Value));
			await table.BulkCopyAsync(
				from rt in
					from th in previousTable
					select new
					{
						th.TagId,
						th.Date,
						th.Text,
						th.Number,
						th.Quality,
						rn = Sql.Ext
							.RowNumber().Over()
							.PartitionBy(th.TagId)
							.OrderByDesc(th.Date)
							.ToValue()
					}
				where rt.rn == 1
				select new TagHistory
				{
					TagId = rt.TagId,
					Date = rt.Date > date ? rt.Date : date,
					Text = rt.Text,
					Number = rt.Number,
					Quality = (short)rt.Quality >= 192 ? TagQuality.Good_LOCF : TagQuality.Bad_LOCF,
				});
		}
	}

	async Task<HistoryTableInfo[]> PostgreSQL_GetHistoryTablesFromSchema()
	{
		var tables = await db.QueryToArrayAsync<HistoryTableWithIndex>($@"
			SELECT t.table_name AS ""Name"", i.indexname AS ""Index""
				FROM information_schema.TABLES t
				LEFT JOIN pg_indexes i ON i.tablename = t.table_name
				WHERE t.table_schema = 'public'
				AND table_name LIKE '{NamePrefix}_%';");

		return tables
			.Select(x => new HistoryTableInfo
			{
				Name = x.Name,
				Date = GetTableDate(x.Name),
				HasIndex = !string.IsNullOrEmpty(x.Index),
			})
			.ToArray();
	}

	async Task PostgreSQL_CreateHistoryIndex(string tableName)
	{
		await db.ExecuteAsync($"CREATE INDEX {tableName.ToLower()}{IndexPostfix} " +
			$"ON public.\"{tableName}\" (\"{nameof(TagHistory.TagId)}\", \"{nameof(TagHistory.Date)}\");");
	}

	#endregion
}
