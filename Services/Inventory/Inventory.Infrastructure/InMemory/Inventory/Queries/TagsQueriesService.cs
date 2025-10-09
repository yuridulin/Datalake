using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.Tags;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Queries;

namespace Datalake.Inventory.Infrastructure.InMemory.Inventory.Queries;

public class TagsQueriesService(IInventoryCache inventoryCache) : ITagsQueriesService
{
	public Task<TagFullInfo?> GetWithDetailsAsync(int tagId, CancellationToken ct = default)
	{
		var state = inventoryCache.State;

		var data = state.ActiveTags
			.Where(tag => tag.Id == tagId)
			.Select(tag =>
			{
				state.ActiveSourcesById.TryGetValue(tag.SourceId, out var source);
				return new TagFullInfo
				{
					Id = tag.Id,
					Guid = tag.GlobalGuid,
					Name = tag.Name,
					Description = tag.Description,
					Resolution = tag.Resolution,
					Type = tag.Type,
					Formula = tag.Formula,
					Thresholds = state.TagThresholds
						.Where(x => x.TagId == tag.Id)
						.Select(x => new TagThresholdInfo
						{
							Threshold = x.InputValue,
							Result = x.OutputValue,
						})
						.ToList(),
					ThresholdSourceTag = !state.ActiveTagsById.TryGetValue(tag.ThresholdSourceTagId ?? 0, out var thresholdSourceTag) ? null : new TagAsInputInfo
					{
						Id = thresholdSourceTag.Id,
						Resolution = thresholdSourceTag.Resolution,
						Guid = thresholdSourceTag.GlobalGuid,
						Name = thresholdSourceTag.Name,
						Type = thresholdSourceTag.Type,
						BlockId = tag.SourceTagBlockId,
						SourceType = !state.ActiveSourcesById.TryGetValue(thresholdSourceTag.SourceId, out var thresholdSourceTagSource)
							? SourceType.Unset
							: thresholdSourceTagSource.Type,
					},
					FormulaInputs = state.TagInputs
						.Where(relation => relation.TagId == tag.Id)
						.Join(state.ActiveTags, relation => relation.InputTagId, inputTag => inputTag.Id, (relation, inputTag) => new TagInputInfo
						{
							Id = inputTag.Id,
							Guid = inputTag.GlobalGuid,
							Name = inputTag.Name,
							VariableName = relation.VariableName,
							BlockId = relation.InputBlockId,
							Type = inputTag.Type,
							Resolution = inputTag.Resolution,
							SourceType = !state.ActiveSourcesById.TryGetValue(inputTag.SourceId, out var inputTagSource) ? SourceType.Unset : inputTagSource.Type,
						})
						.ToArray(),
					IsScaling = tag.IsScaling,
					MaxEu = tag.MaxEu,
					MaxRaw = tag.MaxRaw,
					MinEu = tag.MinEu,
					MinRaw = tag.MinRaw,
					SourceId = tag.SourceId,
					SourceItem = tag.SourceItem,
					SourceType = source != null ? source.Type : SourceType.Unset,
					SourceName = source != null ? source.Name : "Unknown",
					SourceTag = !state.ActiveTagsById.TryGetValue(tag.SourceTagId ?? 0, out var sourceTag) ? null : new TagAsInputInfo
					{
						Id = sourceTag.Id,
						Resolution = sourceTag.Resolution,
						Guid = sourceTag.GlobalGuid,
						Name = sourceTag.Name,
						Type = sourceTag.Type,
						BlockId = tag.SourceTagBlockId,
						SourceType = !state.ActiveSourcesById.TryGetValue(sourceTag.SourceId, out var sourceTagSource) ? SourceType.Unset : sourceTagSource.Type,
					},
					Aggregation = tag.Aggregation,
					AggregationPeriod = tag.AggregationPeriod,
					Blocks = state.BlockTags
						.Where(relation => relation.TagId == tag.Id)
						.Select(relation => !state.ActiveBlocksById.TryGetValue(relation.BlockId, out var block) ? null : new TagBlockRelationInfo
						{
							Id = block.Id,
							Guid = block.GlobalId,
							Name = block.Name,
							RelationId = relation.Id,
							LocalName = relation.Name,
						})
						.Where(block => block != null)
						.ToArray()!,
				};
			})
			.FirstOrDefault();

		return Task.FromResult(data);
	}

	public Task<IEnumerable<TagInfo>> GetAsync(
		IEnumerable<int>? identifiers,
		IEnumerable<Guid>? guids,
		int? sourceId,
		CancellationToken ct = default)
	{
		var state = inventoryCache.State;

		var data = state.ActiveTags
			.Where(tag => identifiers == null || identifiers.Contains(tag.Id))
			.Where(tag => guids == null || guids.Contains(tag.Guid))
			.Where(tag => sourceId == null || tag.SourceId == sourceId)
			.Select(tag =>
			{
				state.ActiveSourcesById.TryGetValue(tag.SourceId, out var source);
				return new TagInfo
				{
					Id = tag.Id,
					Guid = tag.GlobalGuid,
					Name = tag.Name,
					Description = tag.Description,
					Resolution = tag.Resolution,
					Type = tag.Type,
					Formula = tag.Formula,
					Thresholds = state.TagThresholds
						.Where(x => x.TagId == tag.Id)
						.Select(x => new TagThresholdInfo
						{
							Threshold = x.InputValue,
							Result = x.OutputValue,
						})
						.ToList(),
					ThresholdSourceTag = !state.ActiveTagsById.TryGetValue(tag.ThresholdSourceTagId ?? 0, out var thresholdSourceTag) ? null : new TagAsInputInfo
					{
						Id = thresholdSourceTag.Id,
						Resolution = thresholdSourceTag.Resolution,
						Guid = thresholdSourceTag.GlobalGuid,
						Name = thresholdSourceTag.Name,
						Type = thresholdSourceTag.Type,
						BlockId = tag.SourceTagBlockId,
						SourceType = !state.ActiveSourcesById.TryGetValue(thresholdSourceTag.SourceId, out var thresholdSourceTagSource)
							? SourceType.Unset
							: thresholdSourceTagSource.Type,
					},
					FormulaInputs = state.TagInputs
						.Where(relation => relation.TagId == tag.Id)
						.Join(state.ActiveTags, relation => relation.InputTagId, inputTag => inputTag.Id, (relation, inputTag) => new TagInputInfo
						{
							Id = inputTag.Id,
							Guid = inputTag.GlobalGuid,
							Name = inputTag.Name,
							VariableName = relation.VariableName,
							Type = inputTag.Type,
							Resolution = inputTag.Resolution,
							BlockId = relation.InputBlockId,
							SourceType = !state.ActiveSourcesById.TryGetValue(inputTag.SourceId, out var inputTagSource) ? SourceType.Unset : inputTagSource.Type,
						})
						.ToArray(),
					IsScaling = tag.IsScaling,
					MaxEu = tag.MaxEu,
					MaxRaw = tag.MaxRaw,
					MinEu = tag.MinEu,
					MinRaw = tag.MinRaw,
					SourceId = tag.SourceId,
					SourceItem = tag.SourceItem,
					SourceType = source != null ? source.Type : SourceType.Unset,
					SourceName = source != null ? source.Name : "Unknown",
					SourceTag = !state.ActiveTagsById.TryGetValue(tag.SourceTagId ?? 0, out var sourceTag) ? null : new TagAsInputInfo
					{
						Id = sourceTag.Id,
						Resolution = sourceTag.Resolution,
						Guid = sourceTag.GlobalGuid,
						Name = sourceTag.Name,
						Type = sourceTag.Type,
						BlockId = tag.SourceTagBlockId,
						SourceType = !state.ActiveSourcesById.TryGetValue(sourceTag.SourceId, out var sourceTagSource) ? SourceType.Unset : sourceTagSource.Type,
					},
					Aggregation = tag.Aggregation,
					AggregationPeriod = tag.AggregationPeriod,
				};
			});

		return Task.FromResult(data);
	}
}
