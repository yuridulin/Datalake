using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Queries;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Queries.GetTagWithSettingsAndBlocks;

public interface ITagWithSettingsAndBlocksHandler : IQueryHandler<GetTagWithDetailsQuery, TagWithSettingsAndBlocksInfo> { }

public class GetTagWithSettingsAndBlocksHandler(ITagsQueriesService tagsQueriesService) : ITagWithSettingsAndBlocksHandler
{
	public async Task<TagWithSettingsAndBlocksInfo> HandleAsync(GetTagWithDetailsQuery query, CancellationToken ct = default)
	{
		query.User.ThrowIfNoAccessToTag(AccessType.Viewer, query.Id);

		var tagsWithSettings = await tagsQueriesService.GetWithSettingsAsync(identifiers: [query.Id], ct: ct);
		var tagWithSettings = tagsWithSettings.FirstOrDefault()
			?? throw InventoryNotFoundException.NotFoundTag(query.Id);

		var blocks = await tagsQueriesService.GetRelationsToBlocksAsync(tagWithSettings.Id, ct);

		var inputs = await tagsQueriesService.GetInputsAsync([tagWithSettings.Id], ct);
		var thresholds = await tagsQueriesService.GetThresholdsAsync([tagWithSettings.Id], ct);

		var response = new TagWithSettingsAndBlocksInfo
		{
			Id = tagWithSettings.Id,
			Name = tagWithSettings.Name,
			Guid = tagWithSettings.Guid,
			IsScaling = tagWithSettings.IsScaling,
			MaxEu = tagWithSettings.MaxEu,
			MaxRaw = tagWithSettings.MaxRaw,
			MinEu = tagWithSettings.MinEu,
			MinRaw = tagWithSettings.MinRaw,
			Resolution = tagWithSettings.Resolution,
			SourceId = tagWithSettings.SourceId,
			SourceType = tagWithSettings.SourceType,
			Type = tagWithSettings.Type,
			AccessRule = tagWithSettings.AccessRule,
			Aggregation = tagWithSettings.Aggregation,
			AggregationPeriod = tagWithSettings.AggregationPeriod,
			Description = tagWithSettings.Description,
			Formula = tagWithSettings.Formula,
			SourceItem = tagWithSettings.SourceItem,
			SourceName = tagWithSettings.SourceName,
			SourceTag = tagWithSettings.SourceTag,
			ThresholdSourceTag = tagWithSettings.ThresholdSourceTag,

			Blocks = blocks,
			FormulaInputs = inputs,
			Thresholds = thresholds,
		};

		return response;
	}
}
