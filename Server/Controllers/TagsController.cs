using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Tags;
using Datalake.Database.Repositories;
using Datalake.Server.Controllers.Base;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Linq;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с тегами
/// </summary>
/// <param name="tagsRepository">Репозиторий</param>
[ApiController]
[Route("api/[controller]")]
public class TagsController(TagsRepository tagsRepository) : ApiControllerBase
{
	/// <summary>
	/// Создание нового тега
	/// </summary>
	/// <param name="tagCreateRequest">Необходимые данные для создания тега</param>
	/// <returns>Идентификатор нового тега в локальной базе данных</returns>
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest tagCreateRequest)
	{
		var user = Authenticate();

		return await tagsRepository.CreateAsync(user, tagCreateRequest);
	}

	/// <summary>
	/// Получение информации о конкретном теге, включая информацию о источнике и настройках получения данных
	/// </summary>
	/// <param name="guid">Идентификатор тега</param>
	/// <returns>Объект информации о теге</returns>
	/// <exception cref="NotFoundException">Ошибка, если тег не найден</exception>
	[HttpGet("{guid}")]
	public async Task<ActionResult<TagInfo>> ReadAsync(Guid guid)
	{
		var tag = await tagsRepository.GetInfoWithSources()
			.Where(x => x.Guid == guid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Тег {guid}");

		return tag;
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
	public async Task<ActionResult<TagInfo[]>> ReadAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? id,
		[FromQuery] string[]? names,
		[FromQuery] Guid[]? guids)
	{
		var query = tagsRepository.GetInfoWithSources();

		if (sourceId.HasValue)
		{
			query = query.Where(x => sourceId.Value == x.SourceId);
		}
		if (id?.Length == 0)
		{
			query = query.Where(x => id.Contains(x.Id));
		}
		if (names?.Length == 0)
		{
			query = query.Where(x => names.Contains(x.Name));
		}
		if (guids?.Length == 0)
		{
			query = query.Where(x => guids.Contains(x.Guid));
		}

		var tags = await query.ToArrayAsync();

		return tags;
	}

	/// <summary>
	/// Получение списка тегов, подходящих для использования в формулах
	/// </summary>
	/// <returns>Список тегов</returns>
	[HttpGet("inputs")]
	public async Task<ActionResult<TagAsInputInfo[]>> ReadPossibleInputsAsync()
	{
		var tags = await tagsRepository.GetPossibleInputs()
			.ToArrayAsync();

		return tags;
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

		await tagsRepository.UpdateAsync(user, guid, tag);

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

		await tagsRepository.DeleteAsync(user, guid);

		return NoContent();
	}
}
