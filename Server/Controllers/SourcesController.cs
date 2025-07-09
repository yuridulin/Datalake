using Datalake.Database;
using Datalake.Database.Constants;
using Datalake.Database.Functions;
using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Sources;
using Datalake.Server.Controllers.Base;
using Datalake.Server.Services.Maintenance;
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
	DatalakeDerivedDataStore derivedDataStore,
	SourcesMemoryRepository sourcesRepository,
	ReceiverService receiverService,
	TagsStateService tagsStateService) : ApiControllerBase(derivedDataStore)
{
	/// <summary>
	/// Создание источника с информацией по умолчанию
	/// </summary>
	/// <returns>Идентификатор источника</returns>
	[HttpPost("empty")]
	public async Task<ActionResult<SourceInfo>> CreateAsync()
	{
		var user = Authenticate();

		var info = await sourcesRepository.CreateAsync(db, user);

		return info;
	}

	/// <summary>
	/// Создание источника на основе переданных данных
	/// </summary>
	/// <param name="source">Данные нового источника</param>
	/// <returns>Идентификатор источника</returns>
	[HttpPost]
	public async Task<ActionResult<SourceInfo>> CreateAsync(
		[BindRequired, FromBody] SourceInfo source)
	{
		var user = Authenticate();

		var info = await sourcesRepository.CreateAsync(db, user, source);

		return info;
	}

	/// <summary>
	/// Получение данных о источнике
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Данные о источнике</returns>
	/// <exception cref="NotFoundException">Источник не найден по идентификатору</exception>
	[HttpGet("{id:int}")]
	public ActionResult<SourceInfo> Read(
		[BindRequired, FromRoute] int id)
	{
		var user = Authenticate();

		return sourcesRepository.Read(user, id);
	}

	/// <summary>
	/// Получение списка источников
	/// </summary>
	/// <param name="withCustom">Включить ли в список системные источники</param>
	/// <returns>Список источников</returns>
	[HttpGet]
	public ActionResult<SourceInfo[]> Read(bool withCustom = false)
	{
		var user = Authenticate();

		return sourcesRepository.ReadAll(user, withCustom);
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

		await sourcesRepository.UpdateAsync(db, user, id, source);

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

		await sourcesRepository.DeleteAsync(db, user, id);

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
		var user = Authenticate();

		AccessChecks.ThrowIfNoAccessToSource(user, PublicApi.Enums.AccessType.Viewer, id);

		var source = sourcesRepository.Read(user, id);
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
		var user = Authenticate();

		AccessChecks.ThrowIfNoAccessToSource(user, PublicApi.Enums.AccessType.Editor, id);

		var source = sourcesRepository.ReadWithTags(user, id);

		var sourceItemsResponse = await receiverService.GetItemsFromSourceAsync(source.Type, source.Address);
		var sourceItems = sourceItemsResponse.Tags
			.DistinctBy(x => x.Name)
			.ToDictionary(x => x.Name, x => new SourceItemInfo { Path = x.Name, Type = x.Type, Value = x.Value, Quality = x.Quality });

		var sourceTags = source.Tags.ToList();
		var tagsStates = tagsStateService.GetTagsStates();

		var all = sourceTags
			.Select(tag => new SourceEntryInfo
			{
				TagInfo = tag,
				ItemInfo = sourceItems.TryGetValue(tag.Item, out var itemInfo) ? itemInfo : null,
				IsTagInUse = tagsStates.TryGetValue(tag.Id, out var metrics) && metrics.Any(x => !Lists.InnerRequests.Contains(x.Key))
			})
			.Union(sourceItems
				.Where(itemKeyValue => !sourceTags.Select(tag => tag.Item).Contains(itemKeyValue.Key))
				.Select(itemKeyValue => new SourceEntryInfo
				{
					TagInfo = null,
					ItemInfo = itemKeyValue.Value,
					IsTagInUse = false,
				}));

		return all
			.OrderBy(x => x.ItemInfo?.Path)
			.ThenBy(x => x.TagInfo?.Item)
			.ToArray();
	}
}
