using Datalake.Contracts.Public.Models.Tags;
using Datalake.Inventory.Application.Features.Tags.Commands.CreateTag;
using Datalake.Inventory.Application.Features.Tags.Commands.DeleteTag;
using Datalake.Inventory.Application.Features.Tags.Commands.UpdateTag;
using Datalake.Inventory.Application.Features.Tags.Models;
using Datalake.Inventory.Application.Features.Tags.Queries.GetTags;
using Datalake.Inventory.Application.Features.Tags.Queries.GetTagWithDetails;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

/// <summary>
/// Теги
/// </summary>
[ApiController]
[Route("api/tags")]
public class TagsController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// Создание нового тега
	/// </summary>
	/// <param name="request">Необходимые данные для создания тега</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор нового тега в локальной базе данных</returns>
	[HttpPost]
	public async Task<ActionResult<TagInfo>> CreateAsync(
		[BindRequired, FromBody] TagCreateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ICreateTagHandler>();
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
	/// Получение информации о конкретном теге, включая информацию о источнике и настройках получения данных
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Объект информации о теге</returns>
	[HttpGet("{tagId}")]
	public async Task<ActionResult<TagFullInfo>> GetAsync(
		[FromRoute] int tagId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetTagWithDetailsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Id = tagId,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// Получение списка тегов, включая информацию о источниках и настройках получения данных
	/// </summary>
	/// <param name="sourceId">Идентификатор источника. Если указан, будут выбраны теги только этого источника</param>
	/// <param name="tagsId">Список локальных идентификаторов тегов</param>
	/// <param name="tagsGuid">Список глобальных идентификаторов тегов</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Плоский список объектов информации о тегах</returns>
	[HttpGet]
	public async Task<ActionResult<TagInfo[]>> GetAllAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? tagsId,
		[FromQuery] Guid[]? tagsGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetTagsHandler>();
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
	/// Изменение тега
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="request">Новые данные тега</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{tagId}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int tagId,
		[BindRequired, FromBody] TagUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateTagHandler>();
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
	/// Удаление тега
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{tagId}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int tagId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteTagHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			Id = tagId,
		}, ct);

		return NoContent();
	}
}