using Datalake.Contracts.Public.Enums;
using Datalake.DataService.Services.Metrics.Models;
using Datalake.Shared.Application;
using Datalake.PublicApi.Models.Values;
using System.Collections.Concurrent;

namespace Datalake.DataService.Services.Metrics;

/// <summary>
/// Метрики запросов к данным
/// </summary>
[Singleton]
public class RequestsStateService
{
	private readonly ConcurrentDictionary<ValuesRequestKey, ValuesRequestUsage> _storage = [];

	/// <summary>
	/// Запись метрики по запросу
	/// </summary>
	/// <param name="requests">Запросы к данным</param>
	/// <param name="batchDuration">Длительность выполнения</param>
	/// <param name="responses">Запрошенные данные</param>
	public void RecordBatch(
			ValuesRequest[] requests,
			TimeSpan batchDuration,
			List<ValuesResponse> responses)
	{
		foreach (var req in requests)
		{
			// Собираем ключ
			var key = new ValuesRequestKey(
				req.RequestKey,
				req.Tags ?? Array.Empty<Guid>(),
				req.TagsId,
				req.Old,
				req.Young,
				req.Exact,
				req.Resolution ?? TagResolution.NotSet,
				req.Func ?? AggregationFunc.List);

			// Ищем ответ по этому ключу
			var resp = responses.First(r => r.RequestKey == req.RequestKey);
			var valuesCount = resp.Tags.Sum(t => t.Values.Length);

			// Получаем или создаём запись
			var data = _storage.GetOrAdd(key, _ => new ValuesRequestUsage());
			data.RecordCall(batchDuration, valuesCount);
		}
	}

	/// <summary>
	/// Получение метрик
	/// </summary>
	public IReadOnlyDictionary<ValuesRequestKey, ValuesRequestUsageInfo> GetAllStats() => _storage
		.ToDictionary(x => x.Key, x => (ValuesRequestUsageInfo)x.Value);
}