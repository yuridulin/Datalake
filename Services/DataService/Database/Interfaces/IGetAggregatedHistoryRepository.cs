using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Models;

namespace Datalake.DataService.Database.Interfaces;

/// <summary>
/// Работа с преобразованными значениями
/// </summary>
public interface IGetAggregatedHistoryRepository
{
	/// <summary>
	/// Расчет средневзвешенных и взвешенных сумм по тегам. Взвешивание по секундам
	/// </summary>
	/// <param name="identifiers">Идентификаторы тегов</param>
	/// <param name="moment">Момент времени, относительно которого определяется прошедший период</param>
	/// <param name="period">Размер прошедшего периода</param>
	/// <returns>По одному значению на каждый тег</returns>
	/// <exception cref="ForbiddenException">Неподдерживаемый период</exception>
	Task<TagAggregationWeightedValue[]> GetWeightedAggregatedValuesAsync(int[] identifiers, DateTime? moment = null, AggregationPeriod period = AggregationPeriod.Hour);
}