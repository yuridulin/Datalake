using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.Tags;
using Datalake.Inventory.Application.Queries;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class TagsQueriesService(InventoryDbContext context) : ITagsQueriesService
{
	public async Task<TagFullInfo?> GetWithDetailsAsync(int tagId, CancellationToken ct = default)
	{
		var tag = await QueryTagInfo().FirstOrDefaultAsync(x => x.Id == tagId, ct);

		if (tag == null)
			return null;

		var blocks = await context.BlockTags
			.Where(relation => relation.TagId == tagId)
			.Where(relation => relation.Block != null && !relation.Block.IsDeleted)
			.Select(relation => new TagBlockRelationInfo
			{
				Id = relation.Block.Id,
				Guid = relation.Block.GlobalId,
				Name = relation.Block.Name,
				RelationId = relation.Id,
				LocalName = relation.Name,
			})
			.ToArrayAsync(ct);

		return new TagFullInfo
		{
			Id = tag.Id,
			Guid = tag.Guid,
			Name = tag.Name,
			Resolution = tag.Resolution,
			SourceId = tag.SourceId,
			Type = tag.Type,
			SourceType = tag.SourceType,
			FormulaInputs = tag.FormulaInputs,
			IsScaling = tag.IsScaling,
			MaxEu = tag.MaxEu,
			MaxRaw = tag.MaxRaw,
			MinEu = tag.MinEu,
			MinRaw = tag.MinRaw,
			AccessRule = tag.AccessRule,
			Aggregation = tag.Aggregation,
			AggregationPeriod = tag.AggregationPeriod,
			Description = tag.Description,
			Formula = tag.Formula,
			SourceItem = tag.SourceItem,
			SourceName = tag.SourceName,
			SourceTag = tag.SourceTag,
			Thresholds = tag.Thresholds,
			ThresholdSourceTag = tag.ThresholdSourceTag,
			Blocks = blocks,
		};
	}

	public async Task<IEnumerable<TagInfo>> GetAsync(
		IEnumerable<int>? identifiers,
		IEnumerable<Guid>? guids,
		int? sourceId,
		CancellationToken ct = default)
	{
		return await QueryTagInfo()
			.Where(tag => identifiers == null || identifiers.Contains(tag.Id))
			.Where(tag => guids == null || guids.Contains(tag.Guid))
			.Where(tag => sourceId == null || tag.SourceId == sourceId)
			.ToArrayAsync(ct);
	}

	private IQueryable<TagInfo> QueryTagInfo()
	{
		return context.Tags
			.Where(tag => !tag.IsDeleted)
			.AsNoTracking()
			.Select(tag => new TagInfo
			{
				Id = tag.Id,
				Guid = tag.GlobalGuid,
				Name = tag.Name,
				Description = tag.Description,
				Resolution = tag.Resolution,
				Type = tag.Type,
				Formula = tag.Formula,
				Thresholds = tag.Thresholds
					.Select(x => new TagThresholdInfo
					{
						Threshold = x.InputValue,
						Result = x.OutputValue,
					})
					.ToList(),
				ThresholdSourceTag = tag.ThresholdsSourceTag == null || tag.ThresholdsSourceTag.IsDeleted ? null : new TagAsInputInfo
				{
					Id = tag.ThresholdsSourceTag.Id,
					Resolution = tag.ThresholdsSourceTag.Resolution,
					Guid = tag.ThresholdsSourceTag.GlobalGuid,
					Name = tag.ThresholdsSourceTag.Name,
					Type = tag.ThresholdsSourceTag.Type,
					BlockId = tag.ThresholdSourceTagBlockId,
					SourceType = tag.ThresholdsSourceTag.Source == null || tag.ThresholdsSourceTag.Source.IsDeleted
						? SourceType.Unset
						: tag.ThresholdsSourceTag.Source.Type,
				},
				FormulaInputs = tag.Inputs
					.Where(inputRelation => inputRelation.Tag != null && !inputRelation.Tag.IsDeleted)
					.Select(inputRelation => new TagInputInfo
					{
						Id = inputRelation.InputTag!.Id,
						Guid = inputRelation.InputTag.GlobalGuid,
						Name = inputRelation.InputTag.Name,
						VariableName = inputRelation.VariableName,
						Type = inputRelation.InputTag.Type,
						Resolution = inputRelation.InputTag.Resolution,
						BlockId = inputRelation.InputBlockId,
						SourceType = inputRelation.InputTag.Source == null || inputRelation.InputTag.Source.IsDeleted ? SourceType.Unset : inputRelation.InputTag.Source.Type,
					})
					.ToArray(),
				IsScaling = tag.IsScaling,
				MaxEu = tag.MaxEu,
				MaxRaw = tag.MaxRaw,
				MinEu = tag.MinEu,
				MinRaw = tag.MinRaw,
				SourceId = tag.SourceId,
				SourceItem = tag.SourceItem,
				SourceType = tag.Source == null || tag.Source.IsDeleted ? SourceType.Unset : tag.Source.Type,
				SourceName = tag.Source == null || tag.Source.IsDeleted ? "Unknown" : tag.Source.Name,
				SourceTag = tag.AggregationSourceTag == null || tag.AggregationSourceTag.IsDeleted ? null : new TagAsInputInfo
				{
					Id = tag.AggregationSourceTag.Id,
					Resolution = tag.AggregationSourceTag.Resolution,
					Guid = tag.AggregationSourceTag.GlobalGuid,
					Name = tag.AggregationSourceTag.Name,
					Type = tag.AggregationSourceTag.Type,
					BlockId = tag.SourceTagBlockId,
					SourceType = tag.AggregationSourceTag.Source == null || tag.AggregationSourceTag.Source.IsDeleted ? SourceType.Unset : tag.AggregationSourceTag.Source.Type,
				},
				Aggregation = tag.Aggregation,
				AggregationPeriod = tag.AggregationPeriod,
			});
	}
}
