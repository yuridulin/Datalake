using Datalake.InventoryService.Application.Features.Tags.Commands.CreateTag;
using Datalake.InventoryService.Application.Features.Tags.Commands.DeleteTag;
using Datalake.InventoryService.Application.Features.Tags.Commands.UpdateTag;
using Datalake.InventoryService.Application.Features.Tags.Models;
using Datalake.InventoryService.Application.Features.Tags.Queries.GetTags;
using Datalake.InventoryService.Application.Features.Tags.Queries.GetTagWithDetails;
using Datalake.PublicApi.Models.Tags;
using Datalake.Shared.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Api.Controllers;

/// <summary>
/// Теги
/// </summary>
[ApiController]
[Route("api/v1/tags")]
public class TagsController(IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание нового тега
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="request">Необходимые данные для создания тега</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор нового тега в локальной базе данных</returns>
	[HttpPost]
	public async Task<ActionResult<TagInfo>> CreateAsync(
		[FromServices] ICreateTagHandler handler,
		[BindRequired, FromBody] TagCreateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var result = await handler.HandleAsync(new()
		{
			User = user,
			Type = request.TagType,
			BlockId = request.BlockId,
			SourceId = request.SourceId,
			SourceItem = request.SourceItem,
		}, ct);

		return Ok(result);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о конкретном теге, включая информацию о источнике и настройках получения данных
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Объект информации о теге</returns>
	[HttpGet("{tagId}")]
	public async Task<ActionResult<TagFullInfo>> GetAsync(
		[FromServices] IGetTagWithDetailsHandler handler,
		[FromRoute] int tagId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new()
		{
			User = user,
			Id = tagId,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка тегов, включая информацию о источниках и настройках получения данных
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="sourceId">Идентификатор источника. Если указан, будут выбраны теги только этого источника</param>
	/// <param name="tagsId">Список локальных идентификаторов тегов</param>
	/// <param name="tagsGuid">Список глобальных идентификаторов тегов</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Плоский список объектов информации о тегах</returns>
	[HttpGet]
	public async Task<ActionResult<TagInfo[]>> GetAllAsync(
		[FromServices] IGetTagsHandler handler,
		[FromQuery] int? sourceId,
		[FromQuery] int[]? tagsId,
		[FromQuery] Guid[]? tagsGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new()
		{
			User = user,
			SpecificIdentifiers = tagsId,
			SpecificGuids = tagsGuid,
			SpecificSourceId = sourceId,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение тега
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="request">Новые данные тега</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{tagId}")]
	public async Task<ActionResult> UpdateAsync(
		[FromServices] IUpdateTagHandler handler,
		[BindRequired, FromRoute] int tagId,
		[BindRequired, FromBody] TagUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new()
		{
			User = user,
			Id = tagId,
			Name = request.Name,
			Type = request.Type,
			Description = request.Description,
			SourceId = request.SourceId,
			Resolution = request.Resolution,
			SourceItem = request.SourceItem,
			IsScaling = request.IsScaling,
			MaxEu = request.MaxEu,
			MaxRaw = request.MaxRaw,
			MinEu = request.MinEu,
			MinRaw = request.MinRaw,
			Aggregation = request.Aggregation,
			AggregationPeriod = request.AggregationPeriod,
			SourceTagBlockId = request.SourceTagBlockId,
			SourceTagId = request.SourceTagId,
			Formula = request.Formula,
			FormulaInputs = request.FormulaInputs.Select(x => new TagInputDto()
			{
				TagId = x.TagId,
				BlockId = x.BlockId,
				VariableName = x.VariableName,
			}),
			ThresholdSourceTagId = request.ThresholdSourceTagId,
			ThresholdSourceTagBlockId = request.ThresholdSourceTagBlockId,
			Thresholds = request.Thresholds.Select(x => new TagThresholdDto()
			{
				InputValue = x.Threshold,
				OutputValue = x.Result,
			})
		}, ct);

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление тега
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{tagId}")]
	public async Task<ActionResult> DeleteAsync(
		[FromServices] IDeleteTagHandler handler,
		[BindRequired, FromRoute] int tagId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new()
		{
			User = user,
			Id = tagId,
		}, ct);

		return NoContent();
	}
}