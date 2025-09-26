using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.Values;

namespace Datalake.DataService.Abstractions;

/// <summary>
/// Запись значений ручного ввода по запросу
/// </summary>
public interface IManualWriteValuesService
{
	/// <summary>
	/// Запись переданных значений в историю
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="requests">Список новых значений тегов</param>
	/// <returns>Список текущих значений тегов после записи</returns>
	Task<List<ValuesTagResponse>> WriteAsync(UserAccessEntity user, ValueWriteRequest[] requests);
}