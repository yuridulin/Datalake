using Datalake.Database.Constants;

namespace Datalake.Server.Services.StateManager;

/// <summary>
/// Кэш последних появлений пользователей
/// </summary>
public class UsersStateService
{
	object locker = new();

	/// <summary>
	/// Текущее состояние
	/// </summary>
	public Dictionary<Guid, DateTime> State { get; set; } = [];

	/// <summary>
	/// Запись визита пользователя
	/// </summary>
	/// <param name="guid">Идентификатор пользователя</param>
	public void WriteVisit(Guid guid)
	{
		lock (locker)
		{
			State[guid] = DateFormats.GetCurrentDateTime();
		}
	}
}
