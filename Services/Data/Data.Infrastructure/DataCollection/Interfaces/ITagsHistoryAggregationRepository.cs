using Datalake.Contracts.Public.Enums;
using Datalake.Data.Infrastructure.DataCollection.Models;

namespace Datalake.Data.Infrastructure.DataCollection.Interfaces;

/// <summary>
/// Работа с преобразованными значениями
/// </summary>
public interface ITagsHistoryAggregationRepository
{
	/// <summary>
	/// Расчет средневзвешенных и взвешенных сумм по тегам. Взвешивание по секундам
	/// </summary>
	/// <param name="tagsIdentifiers">Идентификаторы тегов</param>
	/// <param name="date">Момент времени, относительно которого определяется прошедший период</param>
	/// <param name="period">Размер прошедшего периода</param>
	/// <returns>По одному значению на каждый тег</returns>
	Task<TagHistoryAggregationWeightedValue[]> GetWeightedAggregatedValuesAsync(int[] tagsIdentifiers, DateTime? date = null, TagResolution period = TagResolution.Hour);
}
