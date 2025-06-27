using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Values;
using System.Collections.Concurrent;

namespace Datalake.Server.Services.Maintenance;

/// <summary>
/// Кэш обращений к тегам
/// </summary>
public class TagsStateService
{
	private ConcurrentDictionary<Guid, Dictionary<string, DateTime>> _states = [];

	/// <summary>
	/// Получение информации о запросах к тегам
	/// </summary>
	/// <returns></returns>
	public Dictionary<Guid, Dictionary<string, DateTime>> GetTagsStates()
		=> _states.ToDictionary(x => x.Key, x => new Dictionary<string, DateTime>(x.Value));

	/// <summary>
	/// Добавление информации о запросах к тегам
	/// </summary>
	/// <param name="tagGuid">Идентификатор тега</param>
	/// <param name="requestKey">Код запроса</param>
	public void UpdateTagState(Guid tagGuid, string requestKey)
	{
		var now = DateFormats.GetCurrentDateTime();

		_states.AddOrUpdate(
			tagGuid,
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
			if (request.Tags == null)
				continue;

			foreach (var tagGuid in request.Tags)
			{
				UpdateTagState(tagGuid, request.RequestKey);
			}
		}
	}
}