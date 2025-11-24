using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Infrastructure.Database.Extensions;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class TagsQueriesService(InventoryDbLinqContext context) : ITagsQueriesService
{
	public async Task<TagSimpleInfo[]> GetAsync(
		IEnumerable<int>? identifiers = null,
		IEnumerable<Guid>? guids = null,
		TagType? type = null,
		int? sourceId = null,
		CancellationToken ct = default)
	{
		var query = context.Tags.AsSimpleInfo(context.Sources);

		if (identifiers?.Count() > 0)
			query = query.Where(x => identifiers.Contains(x.Id));
		if (guids?.Count() > 0)
			query = query.Where(x => guids.Contains(x.Guid));
		if (type != null)
			query = query.Where(x => x.Type == type);
		if (sourceId != null)
			query = query.Where(x => x.SourceId == sourceId);

		return await query.ToArrayAsync(ct);
	}

	public async Task<TagWithSettingsInfo[]> GetWithSettingsAsync(
		IEnumerable<int>? identifiers = null,
		IEnumerable<Guid>? guids = null,
		TagType? type = null,
		int? sourceId = null,
		CancellationToken ct = default)
	{
		var query =
			from tag in context.Tags
			from source in context.Sources.InnerJoin(x => x.Id == tag.SourceId)
			from aggregationTag in context.Tags.AsSimpleInfo(context.Sources).LeftJoin(x => x.Id == tag.SourceTagId)
			from thresholdTag in context.Tags.AsSimpleInfo(context.Sources).LeftJoin(x => x.Id == tag.ThresholdSourceTagId)
			select new TagWithSettingsInfo
			{
				Id = tag.Id,
				Guid = tag.Guid,
				Name = tag.Name,
				Aggregation = tag.Aggregation,
				AggregationPeriod = tag.AggregationPeriod,
				Description = tag.Description,
				Formula = tag.Formula,
				IsScaling = tag.IsScaling,
				MaxEu = tag.MaxEu,
				MaxRaw = tag.MaxRaw,
				MinEu = tag.MinEu,
				MinRaw = tag.MinRaw,
				Resolution = tag.Resolution,
				SourceId = source.Id,
				SourceType = source.Type,
				Type = tag.Type,
				SourceItem = tag.SourceItem,
				SourceName = source.Name,
				SourceTag = aggregationTag == null ? null : new TagAsInputInfo
				{
					Id = aggregationTag.Id,
					Guid = aggregationTag.Guid,
					Name = aggregationTag.Name,
					Resolution = aggregationTag.Resolution,
					Type = aggregationTag.Type,
					SourceId = aggregationTag.Id,
					SourceType = aggregationTag.SourceType,
					BlockId = tag.SourceTagBlockId,
					Description = aggregationTag.Description,
				},
				ThresholdSourceTag = thresholdTag == null ? null : new TagAsInputInfo
				{
					Id = thresholdTag.Id,
					Guid = thresholdTag.Guid,
					Name = thresholdTag.Name,
					Resolution = thresholdTag.Resolution,
					Type = thresholdTag.Type,
					SourceId = thresholdTag.Id,
					SourceType = thresholdTag.SourceType,
					BlockId = tag.ThresholdSourceTagBlockId,
					Description = thresholdTag.Description,
				},
			};

		if (identifiers?.Count() > 0)
			query = query.Where(x => identifiers.Contains(x.Id));
		if (guids?.Count() > 0)
			query = query.Where(x => guids.Contains(x.Guid));
		if (type != null)
			query = query.Where(x => x.Type == type);
		if (sourceId != null)
			query = query.Where(x => x.SourceId == sourceId);

		return await query.ToArrayAsync(ct);
	}

	public async Task<TagInputInfo[]> GetInputsAsync(IEnumerable<int> tagsId, CancellationToken ct = default)
	{
		var query =
			from input in context.TagInputs
			from tag in context.Tags.AsSimpleInfo(context.Sources).LeftJoin(x => x.Id == input.InputTagId)
			where tagsId.Contains(input.TagId)
			select new TagInputInfo
			{
				VariableName = input.VariableName,
				BlockId = input.InputBlockId,
				Tag = tag,
			};

		return await query.ToArrayAsync(ct);
	}

	public async Task<TagThresholdInfo[]> GetThresholdsAsync(IEnumerable<int> tagsId, CancellationToken ct = default)
	{
		var query =
			from threshold in context.TagThresholds
			where tagsId.Contains(threshold.TagId)
			select new TagThresholdInfo
			{
				Threshold = threshold.InputValue,
				Result = threshold.OutputValue,
			};

		return await query.ToArrayAsync(ct);
	}

	public async Task<TagBlockRelationInfo[]> GetRelationsToBlocksAsync(int tagId, CancellationToken ct = default)
	{
		var query =
			from relation in context.BlockTags
			from block in context.Blocks.AsSimpleInfo().LeftJoin(x => x.Id == relation.BlockId)
			where relation.TagId == tagId
			select new TagBlockRelationInfo
			{
				LocalName = relation.Name,
				RelationId = relation.Id,
				Block = block,
			};

		return await query.ToArrayAsync(ct);
	}
}
