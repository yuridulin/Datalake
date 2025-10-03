using Datalake.DataService.Abstractions;
using Datalake.Shared.Application;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Values;
using System.Collections.Concurrent;

namespace Datalake.DataService.Services.Metrics;

/// <summary>
/// Кэш обращений к тегам
/// </summary>
[Singleton]
public class TagsStateService(
	ITagsStore tagsStore)
{
	private ConcurrentDictionary<int, Dictionary<string, DateTime>> _states = [];

	/// <summary>
	/// Получение информации о запросах к тегам
	/// </summary>
	/// <returns></returns>
	public Dictionary<int, Dictionary<string, DateTime>> GetTagsStates()
		=> _states.ToDictionary(x => x.Key, x => new Dictionary<string, DateTime>(x.Value));

	/// <summary>
	/// Добавление информации о запросах к тегам
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="requestKey">Код запроса</param>
	public void UpdateTagState(int tagId, string requestKey)
	{
		var now = DateFormats.GetCurrentDateTime();

		_states.AddOrUpdate(
			tagId,
			guid => new Dictionary<string, DateTime>
			{
				{ requestKey, now },
			},
			(guid, requests) =>
			{
				var newRequests = new Dictionary<string, DateTime>(requests)
				{
					[requestKey] = now
				};
				return newRequests;
			});
	}

	/// <summary>
	/// Добавление информации о запросах к тегам
	/// </summary>
	/// <param name="requests">Запросы</param>
	public void UpdateTagState(ValuesRequest[] requests)
	{
		foreach (var request in requests)
		{
			var tagsId = request.TagsId;

			if (tagsId == null)
			{
				if (request.Tags != null)
				{
					tagsId = request.Tags.Select(guid => tagsStore.TryGet(guid)?.Id ?? 0).ToArray();
				}
				else
					continue;
			}

			foreach (var tagId in tagsId)
			{
				UpdateTagState(tagId, request.RequestKey);
			}
		}
	}
}