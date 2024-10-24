using Datalake.Database.Models.Values;
using Datalake.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с данными тегов
/// </summary>
[ApiController]
[Route("api/Tags/[controller]")]
public class ValuesController(DatalakeContext db) : ControllerBase
{
	/// <summary>
	/// Путь для получения текущих данные
	/// </summary>
	public const string LiveUrl = "api/Tags/values";

	/// <summary>
	/// Получение значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов с настройками</param>
	/// <param name="energoId">Идентификатор учетной записи EnergoId, от имени которой совершается действие</param>
	/// <returns>Список ответов на запросы</returns>
	[HttpPost]
	public async Task<List<ValuesResponse>> GetAsync(
		[BindRequired, FromBody] ValuesRequest[] requests,
		Guid? energoId = null)
	{
		var responses = await db.ValuesRepository.GetValuesAsync(requests, energoId: energoId);

		return responses;
	}

	/// <summary>
	/// Запись значений на основании списка запросов
	/// </summary>
	/// <param name="requests">Список запросов на изменение</param>
	/// <param name="energoId">Идентификатор учетной записи EnergoId, от имени которой совершается действие</param>
	/// <returns>Список измененных начений</returns>
	[HttpPut]
	public async Task<List<ValuesTagResponse>> WriteAsync(
		[BindRequired, FromBody] ValueWriteRequest[] requests,
		Guid? energoId = null)
	{
		// Флаг отключает проверку на новизну значения по сравнению с текущим
		var responses = await db.ValuesRepository.WriteValuesAsync(requests, overrided: true, energoId: energoId);

		return responses;
	}
}
