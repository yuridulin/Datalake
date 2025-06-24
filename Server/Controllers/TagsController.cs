using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.Database.Repositories;
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
	TagsMemoryRepository tagsRepository) : ApiControllerBase
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
	/// <param name="guid">Идентификатор тега</param>
	/// <returns>Объект информации о теге</returns>
	[HttpGet("{guid}")]
	public async Task<ActionResult<TagInfo>> ReadAsync(Guid guid)
	{
		var user = Authenticate();

		return await TagsRepository.ReadAsync(db, user, guid);
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
	public async Task<ActionResult<TagInfo[]>> ReadAllAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? id,
		[FromQuery] string[]? names,
		[FromQuery] Guid[]? guids)
	{
		var user = Authenticate();

		return await TagsRepository.ReadAllAsync(db, user, sourceId, id, names, guids);
	}

	/// <summary>
	/// Получение списка тегов, подходящих для использования в формулах
	/// </summary>
	/// <param name="guid">Идентификатор тега</param>
	/// <returns>Список тегов</returns>
	[HttpGet("{guid}/inputs")]
	public async Task<ActionResult<TagAsInputInfo[]>> ReadPossibleInputsAsync(
		[BindRequired, FromRoute] Guid guid)
	{
		var user = Authenticate();

		return await TagsRepository.ReadPossibleInputsAsync(db, user, guid);
	}

	/// <summary>
	/// Изменение тега
	/// </summary>
	/// <param name="guid">Идентификатор тега</param>
	/// <param name="tag">Новые данные тега</param>
	[HttpPut("{guid}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid guid,
		[BindRequired, FromBody] TagUpdateRequest tag)
	{
		var user = Authenticate();

		await tagsRepository.UpdateAsync(db, user, guid, tag);

		return NoContent();
	}

	/// <summary>
	/// Удаление тега
	/// </summary>
	/// <param name="guid">Идентификатор тега</param>
	[HttpDelete("{guid}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid guid)
	{
		var user = Authenticate();

		await tagsRepository.DeleteAsync(db, user, guid);

		return NoContent();
	}
}
