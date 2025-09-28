using Datalake.InventoryService.Domain.Constants;
using Datalake.InventoryService.Domain.Queries;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.InventoryService.Infrastructure.Cache.Inventory.Services;

public class SourceQueriesService(IInventoryCache inventoryCache) : ISourceQueriesService
{
	public Task<IEnumerable<SourceInfo>> GetAsync(bool withCustom = false)
	{
		var state = inventoryCache.State;

		var data = state.Sources
			.Where(source => !source.IsDeleted && (withCustom || !Lists.CustomSources.Contains(source.Type)))
			.Select(source => new SourceInfo
			{
				Id = source.Id,
				Name = source.Name,
				Address = source.Address,
				Description = source.Description,
				Type = source.Type,
				IsDisabled = source.IsDisabled,
			});

		return Task.FromResult(data);
	}

	public Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAsync()
	{
		var state = inventoryCache.State;

		var data = state.Sources
			.Where(source => !source.IsDeleted)
			.Select(source => new SourceWithTagsInfo
			{
				Id = source.Id,
				Address = source.Address,
				Name = source.Name,
				Type = source.Type,
				IsDisabled = source.IsDisabled,
				Tags = state.Tags
					.Where(tag => !tag.IsDeleted && tag.SourceId == source.Id)
					.Select(tag => new SourceTagInfo
					{
						Id = tag.Id,
						Guid = tag.GlobalGuid,
						Item = tag.SourceItem ?? string.Empty,
						FormulaInputs = Array.Empty<SourceTagInfo.TagInputMinimalInfo>(),
						Name = tag.Name,
						Type = tag.Type,
						Resolution = tag.Resolution,
						SourceType = source.Type,
						Aggregation = tag.Aggregation,
						AggregationPeriod = tag.AggregationPeriod,
					})
					.ToArray(),
			});

		return Task.FromResult(data);
	}

	public Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAndSourceTagsAsync()
	{
		var state = inventoryCache.State;

		var data = state.Sources
			.Where(source => !source.IsDeleted)
			.Select(source => new SourceWithTagsInfo
			{
				Id = source.Id,
				Address = source.Address,
				Name = source.Name,
				Type = source.Type,
				IsDisabled = source.IsDisabled,
				Tags = state.Tags
					.Where(tag => !tag.IsDeleted && tag.SourceId == source.Id)
					.Select(tag => new SourceTagInfo
					{
						Id = tag.Id,
						Guid = tag.GlobalGuid,
						Item = tag.SourceItem ?? string.Empty,
						Calculation = tag.Calculation,
						Formula = tag.Formula,
						Thresholds = tag.Thresholds,
						ThresholdSourceTag = !state.TagsById.TryGetValue(tag.ThresholdSourceTagId ?? 0, out var thresholdSourceTag) ? null :
							new SourceTagInfo.TagInputMinimalInfo
							{
								InputTagId = thresholdSourceTag.Id,
								InputTagType = thresholdSourceTag.Type,
								VariableName = thresholdSourceTag.Name
							},
						FormulaInputs = state.TagInputs
							.Where(input => input.TagId == tag.Id)
							.Select(input => !state.TagsById.TryGetValue(input.InputTagId ?? 0, out var inputTag) ? null :
								new SourceTagInfo.TagInputMinimalInfo
								{
									InputTagId = inputTag.Id,
									InputTagType = inputTag.Type,
									VariableName = input.VariableName,
								})
							.Where(x => x != null)
							.ToArray()!,
						Name = tag.Name,
						Type = tag.Type,
						Resolution = tag.Resolution,
						SourceType = source.Type,
						Aggregation = tag.Aggregation,
						AggregationPeriod = tag.AggregationPeriod,
						SourceTag = !state.TagsById.TryGetValue(tag.SourceTagId ?? 0, out var sourceTag) ? null :
							new SourceTagInfo.TagInputMinimalInfo
							{
								InputTagId = sourceTag.Id,
								InputTagType = sourceTag.Type,
								VariableName = sourceTag.Name
							},
					})
					.ToArray(),
			});

		return Task.FromResult(data);
	}
}
