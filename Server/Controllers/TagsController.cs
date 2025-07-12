using Datalake.Database;
using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Models.Tags;
using Datalake.Server.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с тегами
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TagsController(
	DatalakeContext db,
	DatalakeDerivedDataStore derivedDataStore,
	TagsMemoryRepository tagsRepository) : ApiControllerBase(derivedDataStore)
{
	/// <summary>
	/// Создание нового тега
	/// </summary>
	/// <param name="tagCreateRequest">Необходимые данные для создания тега</param>
	/// <returns>Идентификатор нового тега в локальной базе данных</returns>
	[HttpPost]
	public async Task<ActionResult<TagInfo>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest tagCreateRequest)
	{
		var user = Authenticate();

		return await tagsRepository.CreateAsync(db, user, tagCreateRequest);
	}

	/// <summary>
	/// Получение информации о конкретном теге, включая информацию о источнике и настройках получения данных
	/// </summary>
	/// <param name="id">Идентификатор тега</param>
	/// <returns>Объект информации о теге</returns>
	[HttpGet("{id}")]
	public ActionResult<TagFullInfo> Read(int id)
	{
		var user = Authenticate();

		return tagsRepository.Read(user, id);
	}

	/// <summary>
	/// Получение списка тегов, включая информацию о источниках и настройках получения данных
	/// </summary>
	/// <param name="sourceId">Идентификатор источника. Если указан, будут выбраны теги только этого источника</param>
	/// <param name="id">Список локальных идентификаторов тегов</param>
	/// <param name="names">Список текущих наименований тегов</param>
	/// <param name="guids">Список глобальных идентификаторов тегов</param>
	/// <returns>Плоский список объектов информации о тегах</returns>
	[HttpGet]
	public ActionResult<TagInfo[]> ReadAll(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? id,
		[FromQuery] string[]? names,
		[FromQuery] Guid[]? guids)
	{
		var user = Authenticate();

		return tagsRepository.ReadAll(user, sourceId, id, names, guids);
	}

	/// <summary>
	/// Изменение тега
	/// </summary>
	/// <param name="id">Идентификатор тега</param>
	/// <param name="tag">Новые данные тега</param>
	[HttpPut("{id}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] TagUpdateRequest tag)
	{
		var user = Authenticate();

		await tagsRepository.UpdateAsync(db, user, id, tag);

		return NoContent();
	}

	/// <summary>
	/// Удаление тега
	/// </summary>
	/// <param name="id">Идентификатор тега</param>
	[HttpDelete("{id}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = Authenticate();

		await tagsRepository.DeleteAsync(db, user, id);

		return NoContent();
	}
}
