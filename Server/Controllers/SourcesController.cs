using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Sources;
using Datalake.Database;
using Datalake.Server.Controllers.Base;
using Datalake.Server.Services.Receiver;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с источниками данных
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SourcesController(
	DatalakeContext db,
	ReceiverService receiverService) : ApiControllerBase
{
	/// <summary>
	/// Создание источника с информацией по умолчанию
	/// </summary>
	/// <returns>Идентификатор источника</returns>
	[HttpPost("empty")]
	public async Task<ActionResult<int>> CreateAsync()
	{
		var user = Authenticate();

		var id = await db.SourcesRepository.CreateAsync(user);

		return id;
	}

	/// <summary>
	/// Создание источника на основе переданных данных
	/// </summary>
	/// <param name="source">Данные нового источника</param>
	/// <returns>Идентификатор источника</returns>
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[BindRequired, FromBody] SourceInfo source)
	{
		var user = Authenticate();
		var id = await db.SourcesRepository.CreateAsync(user, source);

		return id;
	}

	/// <summary>
	/// Получение данных о источнике
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Данные о источнике</returns>
	/// <exception cref="NotFoundException">Источник не найден по идентификатору</exception>
	[HttpGet("{id:int}")]
	public async Task<ActionResult<SourceInfo>> ReadAsync(
		[BindRequired, FromRoute] int id)
	{
		return await db.SourcesRepository.GetInfo()
			.FirstOrDefaultAsync(x => x.Id == id)
			?? throw new NotFoundException($"Источник #{id}");
	}

	/// <summary>
	/// Получение списка источников
	/// </summary>
	/// <param name="withCustom">Включить ли в список системные источники</param>
	/// <returns>Список источников</returns>
	[HttpGet]
	public async Task<ActionResult<SourceInfo[]>> ReadAsync(bool withCustom = false)
	{
		return await db.SourcesRepository.GetInfo(withCustom)
			.ToArrayAsync();
	}

	/// <summary>
	/// Изменение источника
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <param name="source">Новые данные источника</param>
	[HttpPut("{id:int}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] SourceInfo source)
	{
		var user = Authenticate();
		await db.SourcesRepository.UpdateAsync(user, id, source);

		return NoContent();
	}

	/// <summary>
	/// Удаление источника
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	[HttpDelete("{id:int}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = Authenticate();
		await db.SourcesRepository.DeleteAsync(user, id);

		return NoContent();
	}

	/// <summary>
	/// Получение доступных значений источника
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Список данных источника</returns>
	/// <exception cref="NotFoundException"></exception>
	[HttpGet("{id:int}/items")]
	public async Task<ActionResult<SourceItemInfo[]>> GetItemsAsync(
		[BindRequired, FromRoute] int id)
	{
		var source = await db.SourcesRepository.GetInfo()
			.Where(x => x.Id == id)
			.Select(x => new { x.Type, x.Address })
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Источник #{id}");

		var sourceItemsResponse = await receiverService.GetItemsFromSourceAsync(source.Type, source.Address);

		var items = sourceItemsResponse.Tags
			.Select(x => new SourceItemInfo
			{
				Type = x.Type,
				Path = x.Name,
				Value = x.Value,
			})
			.OrderBy(x => x.Path)
			.ToArray();

		return items;
	}

	/// <summary>
	/// Получение доступных значений и связанных тегов источника
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Список данных источника</returns>
	/// <exception cref="NotFoundException"></exception>
	[HttpGet("{id:int}/items-and-tags")]
	public async Task<ActionResult<SourceEntryInfo[]>> GetItemsWithTagsAsync(
		[BindRequired, FromRoute] int id)
	{
		var source = await db.SourcesRepository.GetInfo()
			.Where(x => x.Id == id)
			.Select(x => new { x.Type, x.Address })
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Источник #{id}");

		var sourceItemsResponse = await receiverService.GetItemsFromSourceAsync(source.Type, source.Address);
		var sourceItems = sourceItemsResponse.Tags
			.DistinctBy(x => x.Name)
			.ToDictionary(x => x.Name, x => new SourceItemInfo { Path = x.Name, Type = x.Type, Value = x.Value, Quality = x.Quality });

		var sourceTags = await db.SourcesRepository.GetExistTags(id)
			.ToListAsync();

		var all = sourceTags.Select(tag => new SourceEntryInfo
		{
			TagInfo = tag,
			ItemInfo = sourceItems.TryGetValue(tag.Item, out var itemInfo) ? itemInfo : null,
		})
			.Union(sourceItems
				.Where(itemKeyValue => !sourceTags.Select(tag => tag.Item).Contains(itemKeyValue.Key))
				.Select(itemKeyValue => new SourceEntryInfo
				{
					TagInfo = null,
					ItemInfo = itemKeyValue.Value,
				}));

		return all
			.OrderBy(x => x.ItemInfo?.Path)
			.ThenBy(x => x.TagInfo?.Item)
			.ToArray();
	}
}
