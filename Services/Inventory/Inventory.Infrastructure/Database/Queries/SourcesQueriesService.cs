using Datalake.Contracts.Models.Sources;
using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Queries;
using LinqToDB;

namespace Datalake.Inventory.Infrastructure.Database.Queries;

public class SourcesQueriesService(InventoryDbLinqContext context) : ISourcesQueriesService
{
	private IQueryable<SourceInfo> QuerySourceInfo(bool withCustom = false)
	{
		return context.Sources
			.Where(source => withCustom || !Source.InternalSources.Contains(source.Type))
			.Select(source => new SourceInfo
			{
				Id = source.Id,
				Name = source.Name,
				Address = source.Address,
				Description = source.Description,
				Type = source.Type,
				IsDisabled = source.IsDisabled,
			});
	}

	private IQueryable<SourceTagInfo> QuerySourceTagWithRelationsInfo()
	{
		return context.Tags
			.Select(tag => new SourceTagInfo
			{
				Id = tag.Id,
				Guid = tag.Guid,
				Item = tag.SourceItem ?? string.Empty,
				Formula = tag.Formula,
				Name = tag.Name,
				Type = tag.Type,
				Resolution = tag.Resolution,
				SourceType = SourceType.Unset,
				Aggregation = tag.Aggregation,
				AggregationPeriod = tag.AggregationPeriod,
				Thresholds = tag.Thresholds
					.Select(x => new TagThresholdInfo
					{
						Threshold = x.InputValue,
						Result = x.OutputValue,
					})
					.ToList(),
				ThresholdSourceTag = tag.ThresholdsSourceTag == null || tag.ThresholdsSourceTag.IsDeleted ? null :
					new SourceTagInfo.TagInputMinimalInfo
					{
						InputTagId = tag.ThresholdsSourceTag.Id,
						InputTagType = tag.ThresholdsSourceTag.Type,
						VariableName = tag.ThresholdsSourceTag.Name,
					},
				FormulaInputs = tag.Inputs
					.Where(rel => rel.InputTag != null && !rel.InputTag.IsDeleted)
					.Select(rel => new SourceTagInfo.TagInputMinimalInfo
					{
						InputTagId = rel.InputTag!.Id,
						InputTagType = rel.InputTag.Type,
						VariableName = rel.VariableName,
					})
					.Where(x => x != null)
					.ToArray()!,
				SourceTag = tag.AggregationSourceTag == null || tag.AggregationSourceTag.IsDeleted ? null :
					new SourceTagInfo.TagInputMinimalInfo
					{
						InputTagId = tag.AggregationSourceTag.Id,
						InputTagType = tag.AggregationSourceTag.Type,
						VariableName = tag.AggregationSourceTag.Name,
					},
			});
	}

	private IQueryable<SourceTagInfo> QuerySourceTagInfo()
	{
		return context.Tags
			.Select(tag => new SourceTagInfo
			{
				Id = tag.Id,
				Guid = tag.Guid,
				Item = tag.SourceItem ?? string.Empty,
				Formula = tag.Formula,
				Name = tag.Name,
				Type = tag.Type,
				Resolution = tag.Resolution,
				SourceType = SourceType.Unset,
				Aggregation = tag.Aggregation,
				AggregationPeriod = tag.AggregationPeriod,
				FormulaInputs = Array.Empty<SourceTagInfo.TagInputMinimalInfo>(),
			});
	}

	public async Task<IEnumerable<SourceInfo>> GetAsync(bool withCustom = false, CancellationToken ct = default)
	{
		return await QuerySourceInfo(withCustom).ToArrayAsync(ct);
	}

	public async Task<SourceWithTagsInfo?> GetWithTagsAsync(int sourceId, CancellationToken ct = default)
	{
		var source = await QuerySourceInfo().FirstOrDefaultAsync(source => source.Id == sourceId, ct);

		if (source == null)
			return null;

		var tags = await QuerySourceTagInfo()
			.Where(tag => tag.SourceId == source.Id)
			.ToArrayAsync(ct);

		foreach (var tag in tags)
			tag.SourceType = source.Type;

		return new SourceWithTagsInfo
		{
			Id = source.Id,
			Name = source.Name,
			Description = source.Description,
			Address = source.Address,
			IsDisabled = source.IsDisabled,
			AccessRule = source.AccessRule,
			Type = source.Type,
			Tags = tags,
		};
	}

	public async Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAsync(CancellationToken ct = default)
	{
		var sources = await QuerySourceInfo().ToArrayAsync(ct);

		var sourcesId = sources.Select(x => x.Id).ToArray();
		var tags = await QuerySourceTagInfo().Where(x => sourcesId.Contains(x.SourceId)).ToArrayAsync(ct);

		var sourcesWithTags = sources.Select(source => new SourceWithTagsInfo
		{
			Id = source.Id,
			Name = source.Name,
			Description = source.Description,
			Address = source.Address,
			IsDisabled = source.IsDisabled,
			AccessRule = source.AccessRule,
			Type = source.Type,
			Tags = tags
				.Where(x => x.SourceId == source.Id)
				.ToArray(),
		});

		foreach (var source in sourcesWithTags)
		{
			foreach (var tag in source.Tags)
				tag.SourceType = source.Type;
		}

		return sourcesWithTags;
	}

	public async Task<IEnumerable<SourceWithTagsInfo>> GetWithTagsAndSourceTagsAsync(CancellationToken ct = default)
	{
		var sources = await QuerySourceInfo().ToArrayAsync(ct);

		var sourcesId = sources.Select(x => x.Id).ToArray();
		var tags = await QuerySourceTagWithRelationsInfo().Where(x => sourcesId.Contains(x.SourceId)).ToArrayAsync(ct);

		var sourcesWithTags = sources.Select(source => new SourceWithTagsInfo
		{
			Id = source.Id,
			Name = source.Name,
			Description = source.Description,
			Address = source.Address,
			IsDisabled = source.IsDisabled,
			AccessRule = source.AccessRule,
			Type = source.Type,
			Tags = tags
				.Where(x => x.SourceId == source.Id)
				.ToArray(),
		});

		foreach (var source in sourcesWithTags)
		{
			foreach (var tag in source.Tags)
				tag.SourceType = source.Type;
		}

		return sourcesWithTags;
	}
}
