using Datalake.InventoryService.Application.Queries;
using Datalake.InventoryService.Domain.Constants;
using Datalake.InventoryService.Domain.Entities;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.InventoryService.Infrastructure.Cache.Inventory.Services;

public class SourceQueriesService(IInventoryCache inventoryCache) : ISourceQueriesService
{
	public Task<IEnumerable<SourceInfo>> GetAsync(bool withCustom = false)
	{
		var state = inventoryCache.State;

		var data = state.ActiveSources
			.Where(source => withCustom || !Lists.CustomSources.Contains(source.Type))
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

	public Task<SourceWithTagsInfo?> GetWithTagsAsync(int sourceId)
	{
		var state = inventoryCache.State;

		var data = state.ActiveSources
			.Where(source => source.Id == sourceId)
			.Select(source => MapSourceToSourceWithTagsInfo(state, source))
			.FirstOrDefault();

		return Task.FromResult(data);
	}

	public Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAsync()
	{
		var state = inventoryCache.State;

		var data = state.ActiveSources
			.Select(source => MapSourceToSourceWithTagsInfo(state, source));

		return Task.FromResult(data);
	}

	public Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAndSourceTagsAsync()
	{
		var state = inventoryCache.State;

		var data = state.ActiveSources
			.Select(source => new SourceWithTagsInfo
			{
				Id = source.Id,
				Address = source.Address,
				Name = source.Name,
				Type = source.Type,
				IsDisabled = source.IsDisabled,
				Tags = state.ActiveTags
					.Where(tag => tag.SourceId == source.Id)
					.Select(tag => new SourceTagInfo
					{
						Id = tag.Id,
						Guid = tag.Guid,
						Item = tag.SourceItem ?? string.Empty,
						Calculation = tag.Calculation,
						Formula = tag.Formula,
						Thresholds = tag.Thresholds,
						ThresholdSourceTag = !state.ActiveTagsById.TryGetValue(tag.ThresholdSourceTagId ?? 0, out var thresholdSourceTag) ? null :
							new SourceTagInfo.TagInputMinimalInfo
							{
								InputTagId = thresholdSourceTag.Id,
								InputTagType = thresholdSourceTag.Type,
								VariableName = thresholdSourceTag.Name
							},
						FormulaInputs = state.TagInputs
							.Where(input => input.TagId == tag.Id)
							.Select(input => !state.ActiveTagsById.TryGetValue(input.InputTagId ?? 0, out var inputTag) ? null :
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
						SourceTag = !state.ActiveTagsById.TryGetValue(tag.SourceTagId ?? 0, out var sourceTag) ? null :
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

	private static SourceWithTagsInfo MapSourceToSourceWithTagsInfo(InventoryState currentState, SourceEntity source)
	{
		return new SourceWithTagsInfo
		{
			Id = source.Id,
			Address = source.Address,
			Name = source.Name,
			Type = source.Type,
			IsDisabled = source.IsDisabled,
			Tags = currentState.ActiveTags
				.Where(tag => !tag.IsDeleted && tag.SourceId == source.Id)
				.Select(tag => new SourceTagInfo
				{
					Id = tag.Id,
					Guid = tag.Guid,
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
		};
	}
}
