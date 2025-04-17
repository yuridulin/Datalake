using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Values;

namespace Datalake.Server.Services.StateManager;

/// <summary>
/// Кэш обращений к тегам
/// </summary>
public class TagsStateService
{
	object locker = new();

	private Dictionary<Guid, Dictionary<string, DateTime>> _states = [];

	/// <summary>
	/// Получение информации о запросах к тегам
	/// </summary>
	/// <returns></returns>
	public Dictionary<Guid, Dictionary<string, DateTime>> GetTagsStates()
	{
		return new(_states);
	}

	/// <summary>
	/// Добавление информации о запросах к тегам
	/// </summary>
	/// <param name="tagGuid">Идентификатор тега</param>
	/// <param name="requestKey">Код запроса</param>
	public void UpdateTagState(Guid tagGuid, string requestKey)
	{
		var now = DateFormats.GetCurrentDateTime();

		lock (locker)
		{
			if (_states.TryGetValue(tagGuid, out var value))
			{
				value[requestKey] = now;
			}
			else
			{
				_states[tagGuid] = new()
				{
					[requestKey] = now
				};
			}
		}
	}

	/// <summary>
	/// Добавление информации о запросах к тегам
	/// </summary>
	/// <param name="requests">Запросы</param>
	public void UpdateTagState(ValuesRequest[] requests)
	{
		var now = DateFormats.GetCurrentDateTime();

		lock (locker)
		{
			foreach (var request in requests)
			{
				if (request.Tags == null)
					continue;

				foreach (var tagGuid in request.Tags)
				{
					if (_states.TryGetValue(tagGuid, out var value))
					{
						value[request.RequestKey] = now;
					}
					else
					{
						_states[tagGuid] = new()
						{
							[request.RequestKey] = now
						};
					}
				}
			}
		}
	}
}