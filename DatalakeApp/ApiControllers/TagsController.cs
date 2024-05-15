using DatalakeDatabase.ApiModels.Tags;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController(TagsRepository tagsRepository) : ControllerBase
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
		return await tagsRepository.CreateAsync(tagCreateRequest);
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
	/// <param name="sources">Идентификаторы источников. Если указаны, будут выбраны теги только эти источников</param>
	/// <param name="tags">Имена тегов, которые нужно получить. Если указаны, все прочие будут исключены из выборки</param>
	/// <returns>Плоский список объектов информации о тегах</returns>
	[HttpGet]
	public async Task<ActionResult<TagInfo[]>> ReadAsync(
		[FromQuery] int[]? sources,
		[FromQuery] string[]? tags)
	{
		var query = tagsRepository.GetInfoWithSources();
		if (sources?.Length > 0)
			query = query.Where(x => sources.Contains(x.SourceInfo.Id));
		if (tags?.Length > 0)
			query = query.Where(x => tags.Select(t => t.ToLower()).Contains(x.Name.ToLower()));

		return await query.ToArrayAsync();
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
		[BindRequired, FromBody] TagInfo tag)
	{
		await tagsRepository.UpdateAsync(id, tag);

		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		await tagsRepository.DeleteAsync(id);

		return NoContent();
	}
}
