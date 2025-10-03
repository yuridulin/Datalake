using Datalake.Contracts.Public.Enums;
using Datalake.DataService.Database.Interfaces;
using Datalake.DataService.Extensions;
using Datalake.Inventory.Models;
using Datalake.Shared.Application;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Exceptions;
using LinqToDB;

namespace Datalake.DataService.Database.Repositories;

[Scoped]
public class GetAggregatedHistoryRepository(DataLinqToDbContext db) : IGetAggregatedHistoryRepository
{
	public async Task<TagAggregationWeightedValue[]> GetWeightedAggregatedValuesAsync(
		int[] identifiers,
		DateTime? moment = null,
		AggregationPeriod period = AggregationPeriod.Hour)
	{
		if (identifiers.Length == 0)
			return [];

		// Задаем входные параметры
		var now = moment ?? DateFormats.GetCurrentDateTime();

		DateTime periodStart, periodEnd;
		switch (period)
		{
			case AggregationPeriod.Minute:
				periodEnd = now.RoundByResolution(TagResolution.Minute);
				periodStart = periodEnd.AddMinutes(-1);
				break;

			case AggregationPeriod.Hour:
				periodEnd = now.RoundByResolution(TagResolution.Hour);
				periodStart = periodEnd.AddHours(-1);
				break;

			case AggregationPeriod.Day:
				periodEnd = now.RoundByResolution(TagResolution.Day);
				periodStart = periodEnd.AddDays(-1);
				break;

			default:
				throw new ForbiddenException("задан неподдерживаемый период");
		}

		var source = db.TagsHistory.Where(tag => identifiers.Contains(tag.TagId));

		// CTE: Последнее значение перед началом периода
		var historyBefore =
			from raw in source
			where raw.Date <= periodStart
			select new
			{
				raw.TagId,
				Date = periodStart, // Принудительная установка даты
				raw.Number,
				Order = Sql.Ext.RowNumber()
					.Over()
					.PartitionBy(raw.TagId)
					.OrderByDesc(raw.Date)
					.ToValue()
			} into historyBeforeTemp
			where historyBeforeTemp.Order == 1
			select new
			{
				historyBeforeTemp.TagId,
				historyBeforeTemp.Date,
				historyBeforeTemp.Number
			};

		// CTE: Значения в пределах периода
		var historyBetween =
			from raw in source
			where
				raw.Date > periodStart &&
				raw.Date <= periodEnd
			select new
			{
				raw.TagId,
				raw.Date,
				raw.Number
			};

		// CTE: Объединение двух выборок (UNION ALL)
		var history = historyBefore.Concat(historyBetween);

		// CTE: Добавление следующей даты, чтобы относительно ее посчитать длительность актуальности значения
		var historyWithNext =
			from h in history
			select new
			{
				h.TagId,
				h.Date,
				h.Number,
				NextDate = Sql.Ext.Lead(h.Date, 1, periodEnd)
					.Over()
					.PartitionBy(h.TagId)
					.OrderBy(h.Date)
					.ToValue()
			};

		// CTE: Вычисление длительности действия каждого значения
		var weighted =
			from h in historyWithNext
			select new
			{
				h.TagId,
				h.Number,
				Weight = Sql.DateDiff(Sql.DateParts.Second, h.Date, h.NextDate),
			};

		// Финальный запрос - применяем веса
		var result =
			from w in weighted
			group w by w.TagId into g
			select new TagAggregationWeightedValue
			{
				TagId = g.Key,
				Date = periodEnd,
				SumOfWeights = g.Sum(x => x.Weight) ?? 0,
				SumValuesWithWeights = g.Sum(x => x.Number * x.Weight) ?? 0,
			};

		// Выполнение запроса
		var aggregated = await result.ToArrayAsync();

		return aggregated;
	}
}
