using Datalake.PublicApi.Models.Values;
using Datalake.Shared.Domain.Entities;

namespace Datalake.DataService.Abstractions;

/// <summary>
/// Получение значений по запросу
/// </summary>
public interface IGetValuesService
{
	/// <summary>
	/// Получение значений по списку запрошенных тегов
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="requests">Список запрошенных тегов с настройками получения</param>
	/// <returns>Список ответов со значениями тегов</returns>
	Task<List<ValuesResponse>> GetAsync(UserAccessEntity user, ValuesRequest[] requests);
}
