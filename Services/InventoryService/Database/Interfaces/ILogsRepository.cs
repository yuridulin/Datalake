using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.LogModels;

namespace Datalake.InventoryService.Database.Interfaces;

/// <summary>
/// Репозиторий для работы с сообщениями аудита
/// </summary>
public interface ILogsRepository
{
	/// <summary>
	/// Получение списка сообщений
	/// </summary>
	/// <param name="lastId">Идентификатор сообщения, с которого начать отсчёт количества в сторону более поздних</param>
	/// <param name="firstId">Идентификатор сообщения, с которого начать отсчёт количества в сторону более ранних</param>
	/// <param name="take">Сколько сообщений получить за этот запрос</param>
	/// <param name="sourceId">Идентификатор затронутого источника</param>
	/// <param name="blockId">Идентификатор затронутого блока</param>
	/// <param name="tagGuid">Идентификатор затронутого тега</param>
	/// <param name="userGuid">Идентификатор затронутого пользователя</param>
	/// <param name="groupGuid">Идентификатор затронутой группы пользователей</param>
	/// <param name="categories">Выбранные категории сообщений</param>
	/// <param name="types">Выбранные типы сообщений</param>
	/// <param name="authorGuid">Идентификатор пользователя, создавшего сообщение</param>
	/// <returns>Список сообщений</returns>
	Task<IEnumerable<LogInfo>> GetAsync(int? lastId = null, int? firstId = null, int? take = null, int? sourceId = null, int? blockId = null, Guid? tagGuid = null, Guid? userGuid = null, Guid? groupGuid = null, LogCategory[]? categories = null, LogType[]? types = null, Guid? authorGuid = null);
}