using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tables;
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
	/// Вставка при необходимости значений из предыдущей таблицы
	/// </summary>
	/// <param name="date">дата</param>
	/// <returns>Количество вставленных записей</returns>
	public async Task EnsureInitialValues(DateTime date)
	{
		var initialCount = await WriteInitialValuesAsync(date);

		if (initialCount > 0)
		{
			var tableName = GetTableName(date);
			db.Insert(new Log
			{
				Category = LogCategory.Database,
				Type = LogType.Trace,
				Text = "Для партиции: " + tableName + " добавлены начальные значения. Количество: " + initialCount,
			});
		}
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
		var previousDate = CachedTables.Keys.Where(x => x < date).OrderByDescending(x => x).FirstOrDefault();
		return previousDate == DateTime.MinValue ? null : previousDate;
	}

	internal async Task<ITable<TagHistory>> GetHistoryTableAsync(DateTime seekDate)
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

			var initialCount = await WriteInitialValuesAsync(date);

			db.Insert(new Log
			{
				Category = LogCategory.Database,
				Type = LogType.Trace,
				Text = "Создана партиция: " + tableName + ". Количество начальных значений: " + initialCount,
			});
		}

		return table;
	}

	async Task<long> WriteInitialValuesAsync(DateTime date)
	{
		var table = await db.TablesRepository.GetHistoryTableAsync(date);

		var initialValues = await table
			.Where(x => x.Quality == TagQuality.Bad_LOCF || x.Quality == TagQuality.Good_LOCF)
			.Select(x => x.TagId)
			.Distinct()
			.ToArrayAsync();

		// заполнение начальных значений
		DateTime? previous = CachedTables.Keys.Where(x => x < date).OrderByDescending(x => x).FirstOrDefault();
		if (previous != null && previous != DateTime.MinValue)
		{
			var previousTable = db.GetTable<TagHistory>().TableName(GetTableName(previous.Value));
			var lastValuesQuery =
				from lastValue in
					from value in previousTable
					where !initialValues.Contains(value.TagId)
					select new
					{
						value.TagId,
						value.Date,
						value.Text,
						value.Number,
						value.Quality,
						rn = Sql.Ext
							.RowNumber().Over()
							.PartitionBy(value.TagId)
							.OrderByDesc(value.Date)
							.ToValue()
					}
				where lastValue.rn == 1
				select new TagHistory
				{
					TagId = lastValue.TagId,
					Date = lastValue.Date > date ? lastValue.Date : date,
					Text = lastValue.Text,
					Number = lastValue.Number,
					Quality = (short)lastValue.Quality >= 192 ? TagQuality.Good_LOCF : TagQuality.Bad_LOCF,
				};

			var lastValues = await lastValuesQuery.ToArrayAsync();

			var records = await table.BulkCopyAsync(lastValues);

			return records.RowsCopied;
		}

		return 0;
	}

	async Task<HistoryTableInfo[]> PostgreSQL_GetHistoryTablesFromSchema()
	{
		var tables = await db.QueryToArrayAsync<HistoryTableWithIndex>($@"
			SELECT t.table_name AS ""Name"", i.indexname AS ""Index""
				FROM information_schema.TABLES t
				LEFT JOIN pg_indexes i ON i.tablename = t.table_name
				WHERE t.table_schema = 'public'
				AND table_name LIKE '{NamePrefix}_%';");

		return [.. tables
			.Select(x => new HistoryTableInfo
			{
				Name = x.Name,
				Date = GetTableDate(x.Name),
				HasIndex = !string.IsNullOrEmpty(x.Index),
			})];
	}

	async Task PostgreSQL_CreateHistoryIndex(string tableName)
	{
		await db.ExecuteAsync($"CREATE INDEX {tableName.ToLower()}{IndexPostfix} " +
			$"ON public.\"{tableName}\" (\"{nameof(TagHistory.TagId)}\", \"{nameof(TagHistory.Date)}\");");
	}

	#endregion
}
