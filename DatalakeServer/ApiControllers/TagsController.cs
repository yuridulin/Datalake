using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Tags;
using DatalakeDatabase.Repositories;
using DatalakeServer.ApiControllers.Base;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeServer.ApiControllers;

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
	/// <param name="id">Идентификатор тега</param>
	/// <returns>Объект информации о теге</returns>
	/// <exception cref="NotFoundException">Ошибка, если тег не найден</exception>
	[HttpGet("{id:int}")]
	public async Task<ActionResult<TagInfo>> ReadAsync(int id)
	{
		var tag = await tagsRepository.GetInfoWithSources()
			.Where(x => x.Id == id)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Тег #{id}");

		return tag;
	}

	/// <summary>
	/// Получение списка тегов, включая информацию о источниках и настройках получения данных
	/// </summary>
	/// <param name="sourceId">Идентификатор источника. Если указан, будут выбраны теги только этого источника</param>
	/// <returns>Плоский список объектов информации о тегах</returns>
	[HttpGet]
	public async Task<ActionResult<TagInfo[]>> ReadAsync(
		[FromQuery] int? sourceId)
	{
		var query = tagsRepository.GetInfoWithSources();

		if (sourceId.HasValue)
		{
			query = query.Where(x => sourceId.Value == x.SourceId);
		}

		var tags = await query.ToArrayAsync();

		return tags;
	}

	[HttpGet("inputs")]
	public async Task<ActionResult<TagAsInputInfo[]>> ReadPossibleInputsAsync()
	{
		var tags = await tagsRepository.GetPossibleInputs()
			.ToArrayAsync();

		return tags;
	}

	[HttpPut("{id:int}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] TagUpdateRequest tag)
	{
		var user = Authenticate();

		await tagsRepository.UpdateAsync(user, id, tag);

		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = Authenticate();

		await tagsRepository.DeleteAsync(user, id);

		return NoContent();
	}
}
