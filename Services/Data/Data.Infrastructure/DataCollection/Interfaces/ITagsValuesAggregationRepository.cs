using Datalake.Data.Infrastructure.DataCollection.Models;
using Datalake.Domain.Enums;

namespace Datalake.Data.Infrastructure.DataCollection.Interfaces;

/// <summary>
/// Работа с преобразованными значениями
/// </summary>
public interface ITagsValuesAggregationRepository
{
	/// <summary>
	/// Расчет средневзвешенных и взвешенных сумм по тегам. Взвешивание по секундам
	/// </summary>
	/// <param name="tagsIdentifiers">Идентификаторы тегов</param>
	/// <param name="date">Момент времени, относительно которого определяется прошедший период</param>
	/// <param name="period">Размер прошедшего периода</param>
	/// <returns>По одному значению на каждый тег</returns>
	Task<TagWeightedValue[]> GetWeightedValuesAsync(
		IReadOnlyCollection<int> tagsIdentifiers,
		DateTime? date = null,
		TagResolution period = TagResolution.Hour,
		CancellationToken cancellationToken = default);
}
