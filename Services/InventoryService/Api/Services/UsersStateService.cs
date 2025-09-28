using Datalake.PublicApi.Constants;
using System.Collections.Concurrent;

namespace Datalake.InventoryService.Api.Services;

/// <summary>
/// Кэш последних появлений пользователей
/// </summary>
public class UsersStateService
{
	private ConcurrentDictionary<Guid, DateTime> _state = [];

	/// <summary>
	/// Получение текущего списка визитов пользователей
	/// </summary>
	public Dictionary<Guid, DateTime> State => new(_state);

	/// <summary>
	/// Запись визита пользователя
	/// </summary>
	/// <param name="guid">Идентификатор пользователя</param>
	public void WriteVisit(Guid guid)
	{
		_state.AddOrUpdate(
			guid,
			(_) => DateFormats.GetCurrentDateTime(),
			(_, _) => DateFormats.GetCurrentDateTime());
	}
}
