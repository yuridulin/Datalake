using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Extensions;
using Datalake.Data.Infrastructure.DataCollection.Interfaces;
using Datalake.Data.Infrastructure.DataCollection.Models;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Exceptions;
using LinqToDB;

namespace Datalake.Data.Infrastructure.Database.Repositories;

[Scoped]
public class TagsValuesAggregationRepository(DataDbLinqContext db) : ITagsValuesAggregationRepository
{
	public async Task<TagWeightedValue[]> GetWeightedValuesAsync(
		int[] identifiers,
		DateTime? moment = null,
		TagResolution period = TagResolution.Hour)
	{
		if (identifiers.Length == 0)
			return [];

		// Задаем входные параметры
		var now = moment ?? DateTimeExtension.GetCurrentDateTime();

		DateTime periodStart, periodEnd;
		switch (period)
		{
			case TagResolution.Minute:
				periodEnd = now.RoundByResolution(TagResolution.Minute);
				periodStart = periodEnd.AddMinutes(-1);
				break;

			case TagResolution.Hour:
				periodEnd = now.RoundByResolution(TagResolution.Hour);
				periodStart = periodEnd.AddHours(-1);
				break;

			case TagResolution.Day:
				periodEnd = now.RoundByResolution(TagResolution.Day);
				periodStart = periodEnd.AddDays(-1);
				break;

			default:
				throw new InfrastructureException("При расчете агрегатов указан неподдерживаемый период");
		}

		var source = db.TagsValues.Where(tag => identifiers.Contains(tag.TagId));

		// CTE: Последнее значение перед началом периода
		var valuesBefore =
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
			} into valuesBeforeTemp
			where valuesBeforeTemp.Order == 1
			select new
			{
				valuesBeforeTemp.TagId,
				valuesBeforeTemp.Date,
				valuesBeforeTemp.Number
			};

		// CTE: Значения в пределах периода
		var valuesBetween =
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
		var values = valuesBefore.Concat(valuesBetween);

		// CTE: Добавление следующей даты, чтобы относительно ее посчитать длительность актуальности значения
		var valuesWithNext =
			from h in values
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
			from h in valuesWithNext
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
			select new TagWeightedValue(
				g.Key,
				periodEnd,
				g.Sum(x => x.Weight) ?? 0,
				g.Sum(x => x.Number * x.Weight) ?? 0);

		// Выполнение запроса
		var aggregated = await result.ToArrayAsync();

		return aggregated;
	}
}
