using Datalake.Contracts.Models.Tags;
using Datalake.Contracts.Requests;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Features.Tags.Commands.CreateTag;
using Datalake.Inventory.Application.Features.Tags.Commands.DeleteTag;
using Datalake.Inventory.Application.Features.Tags.Commands.UpdateTag;
using Datalake.Inventory.Application.Features.Tags.Models;
using Datalake.Inventory.Application.Features.Tags.Queries.GetTags;
using Datalake.Inventory.Application.Features.Tags.Queries.GetTagsWithSettings;
using Datalake.Inventory.Application.Features.Tags.Queries.GetTagWithSettingsAndBlocks;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

public class InventoryTagsController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : InventoryTagsControllerBase
{
	public override async Task<ActionResult<TagWithSettingsInfo>> CreateAsync(
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

	public override async Task<ActionResult<TagWithSettingsAndBlocksInfo>> GetWithSettingsAndBlocksAsync(
		[FromRoute] int tagId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ITagWithSettingsAndBlocksHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Id = tagId,
		}, ct);

		return Ok(data);
	}


	public override async Task<ActionResult<TagSimpleInfo[]>> GetAllAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? tagsId,
		[FromQuery] Guid[]? tagsGuid,
		[FromQuery] TagType? type,
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
			SpecificType = type,
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult<TagWithSettingsInfo[]>> GetAllWithSettingsAsync(
		[FromQuery] int? sourceId,
		[FromQuery] int[]? tagsId,
		[FromQuery] Guid[]? tagsGuid,
		[FromQuery] TagType? type,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetTagsWithSettingsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			SpecificIdentifiers = tagsId,
			SpecificGuids = tagsGuid,
			SpecificSourceId = sourceId,
			SpecificType = type,
		}, ct);

		return Ok(data);
	}

	public override async Task<ActionResult> UpdateAsync(
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

	public override async Task<ActionResult> DeleteAsync(
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