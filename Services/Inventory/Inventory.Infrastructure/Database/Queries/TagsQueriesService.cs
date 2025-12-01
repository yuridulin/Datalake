using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using Datalake.Inventory.Infrastructure.Database.Extensions;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class TagsQueriesService(InventoryDbLinqContext context) : ITagsQueriesService
{
	public async Task<List<TagSimpleInfo>> GetAsync(
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

		return await query.ToListAsync(ct);
	}

	public async Task<List<TagWithSettingsInfo>> GetWithSettingsAsync(
		IEnumerable<int>? identifiers = null,
		IEnumerable<Guid>? guids = null,
		TagType? type = null,
		int? sourceId = null,
		CancellationToken ct = default)
	{
		var simpleTagQuery =
			from tag in context.Tags
			from source in context.Sources.InnerJoin(x => x.Id == tag.SourceId)
			select new { Tag = tag, Source = source };

		var query =
			from tag in simpleTagQuery
			from aggregationTag in simpleTagQuery.LeftJoin(x => x.Tag.Id == tag.Tag.SourceTagId)
			from thresholdTag in simpleTagQuery.LeftJoin(x => x.Tag.Id == tag.Tag.ThresholdSourceTagId)
			select new TagWithSettingsInfo
			{
				Id = tag.Tag.Id,
				Guid = tag.Tag.GlobalGuid,
				Name = tag.Tag.Name,
				Aggregation = tag.Tag.Aggregation,
				AggregationPeriod = tag.Tag.AggregationPeriod,
				Description = tag.Tag.Description,
				Formula = tag.Tag.Formula,
				IsScaling = tag.Tag.IsScaling,
				MaxEu = tag.Tag.MaxEu,
				MaxRaw = tag.Tag.MaxRaw,
				MinEu = tag.Tag.MinEu,
				MinRaw = tag.Tag.MinRaw,
				Resolution = tag.Tag.Resolution,
				Type = tag.Tag.Type,
				SourceItem = tag.Tag.SourceItem,

				SourceId = tag.Source.Id,
				SourceType = tag.Source.Type,
				SourceName = tag.Source.Name,

				SourceTag = aggregationTag == null ? null : new TagAsInputInfo
				{
					BlockId = tag.Tag.SourceTagBlockId,

					Id = aggregationTag.Tag.Id,
					Guid = aggregationTag.Tag.GlobalGuid,
					Name = aggregationTag.Tag.Name,
					Resolution = aggregationTag.Tag.Resolution,
					Type = aggregationTag.Tag.Type,
					Description = aggregationTag.Tag.Description,

					SourceId = aggregationTag.Source.Id,
					SourceType = aggregationTag.Source.Type,
				},
				ThresholdSourceTag = thresholdTag == null ? null : new TagAsInputInfo
				{
					BlockId = tag.Tag.ThresholdSourceTagBlockId,

					Id = thresholdTag.Tag.Id,
					Guid = thresholdTag.Tag.GlobalGuid,
					Name = thresholdTag.Tag.Name,
					Resolution = thresholdTag.Tag.Resolution,
					Type = thresholdTag.Tag.Type,
					Description = thresholdTag.Tag.Description,

					SourceId = thresholdTag.Source.Id,
					SourceType = thresholdTag.Source.Type,
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

		return await query.ToListAsync(ct);
	}

	public async Task<List<TagInputInfo>> GetInputsAsync(IEnumerable<int> tagsId, CancellationToken ct = default)
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

		return await query.ToListAsync(ct);
	}

	public async Task<List<TagThresholdInfo>> GetThresholdsAsync(IEnumerable<int> tagsId, CancellationToken ct = default)
	{
		var query =
			from threshold in context.TagThresholds
			where tagsId.Contains(threshold.TagId)
			select new TagThresholdInfo
			{
				Threshold = threshold.InputValue,
				Result = threshold.OutputValue,
			};

		return await query.ToListAsync(ct);
	}

	public async Task<List<TagBlockRelationInfo>> GetRelationsToBlocksAsync(int tagId, CancellationToken ct = default)
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

		return await query.ToListAsync(ct);
	}
}
