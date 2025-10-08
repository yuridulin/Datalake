using Datalake.Domain.Entities;
using Datalake.Inventory.Api.Models.Sources;
using Datalake.Inventory.Api.Models.Tags;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Queries;

namespace Datalake.Inventory.Infrastructure.Cache.Inventory.Queries;

public class SourcesQueriesService(IInventoryCache inventoryCache) : ISourcesQueriesService
{
	public Task<IEnumerable<SourceInfo>> GetAsync(bool withCustom = false, CancellationToken ct = default)
	{
		var state = inventoryCache.State;

		var data = state.ActiveSources
			.Where(source => withCustom || !Source.CustomSources.Contains(source.Type))
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

	public Task<SourceWithTagsInfo?> GetWithTagsAsync(int sourceId, CancellationToken ct = default)
	{
		var state = inventoryCache.State;

		var data = state.ActiveSources
			.Where(source => source.Id == sourceId)
			.Select(source => MapSourceToSourceWithTagsInfo(state, source))
			.FirstOrDefault();

		return Task.FromResult(data);
	}

	public Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAsync(CancellationToken ct = default)
	{
		var state = inventoryCache.State;

		var data = state.ActiveSources
			.Select(source => MapSourceToSourceWithTagsInfo(state, source));

		return Task.FromResult(data);
	}

	public Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAndSourceTagsAsync(CancellationToken ct = default)
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
						Formula = tag.Formula,
						Thresholds = state.TagThresholds
							.Where(x => x.TagId == tag.Id)
							.Select(x => new TagThresholdInfo
							{
								Threshold = x.InputValue,
								Result = x.OutputValue,
							})
							.ToList(),
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

	private static SourceWithTagsInfo MapSourceToSourceWithTagsInfo(IInventoryCacheState currentState, Source source)
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
