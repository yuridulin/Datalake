using Datalake.InventoryService.InMemory.Models;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;
using LinqToDB;

namespace Datalake.Inventory.InMemory.Queries;

/// <summary>
/// Запросы, связанные с тегами
/// </summary>
public static class TagsQueries
{
	/// <summary>
	/// Запрос полной информации о тегах, их блоках и источниках данных
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	public static IEnumerable<TagFullInfo> TagsInfoFull(this DatalakeDataState state)
	{
		var activeTags = state.Tags.Where(tag => !tag.IsDeleted);

		return activeTags
			.Where(tag => !tag.IsDeleted)
			.Select(tag =>
			{
				state.SourcesById.TryGetValue(tag.SourceId, out var source);
				return new TagFullInfo
				{
					Id = tag.Id,
					Guid = tag.GlobalGuid,
					Name = tag.Name,
					Description = tag.Description,
					Resolution = tag.Resolution,
					Type = tag.Type,
					Calculation = tag.Calculation,
					Formula = tag.Formula,
					Thresholds = tag.Thresholds,
					ThresholdSourceTag = !state.TagsById.TryGetValue(tag.ThresholdSourceTagId ?? 0, out var thresholdSourceTag) ? null : new TagAsInputInfo
					{
						Id = thresholdSourceTag.Id,
						Resolution = thresholdSourceTag.Resolution,
						Guid = thresholdSourceTag.GlobalGuid,
						Name = thresholdSourceTag.Name,
						Type = thresholdSourceTag.Type,
						BlockId = tag.SourceTagBlockId,
						SourceType = !state.SourcesById.TryGetValue(thresholdSourceTag.SourceId, out var thresholdSourceTagSource)
							? SourceType.NotSet
							: thresholdSourceTagSource.Type,
					},
					FormulaInputs = state.TagInputs
						.Where(relation => relation.TagId == tag.Id)
						.Join(activeTags, relation => relation.InputTagId, inputTag => inputTag.Id, (relation, inputTag) => new TagInputInfo
						{
							Id = inputTag.Id,
							Guid = inputTag.GlobalGuid,
							Name = inputTag.Name,
							VariableName = relation.VariableName,
							BlockId = relation.InputBlockId,
							Type = inputTag.Type,
							Resolution = inputTag.Resolution,
							SourceType = !state.SourcesById.TryGetValue(inputTag.SourceId, out var inputTagSource) ? SourceType.NotSet : inputTagSource.Type,
						})
						.ToArray(),
					IsScaling = tag.IsScaling,
					MaxEu = tag.MaxEu,
					MaxRaw = tag.MaxRaw,
					MinEu = tag.MinEu,
					MinRaw = tag.MinRaw,
					SourceId = tag.SourceId,
					SourceItem = tag.SourceItem,
					SourceType = source != null ? source.Type : SourceType.NotSet,
					SourceName = source != null ? source.Name : "Unknown",
					SourceTag = !state.TagsById.TryGetValue(tag.SourceTagId ?? 0, out var sourceTag) ? null : new TagAsInputInfo
					{
						Id = sourceTag.Id,
						Resolution = sourceTag.Resolution,
						Guid = sourceTag.GlobalGuid,
						Name = sourceTag.Name,
						Type = sourceTag.Type,
						BlockId = tag.SourceTagBlockId,
						SourceType = !state.SourcesById.TryGetValue(sourceTag.SourceId, out var sourceTagSource) ? SourceType.NotSet : sourceTagSource.Type,
					},
					Aggregation = tag.Aggregation,
					AggregationPeriod = tag.AggregationPeriod,
					Blocks = state.BlockTags
						.Where(relation => relation.TagId == tag.Id)
						.Select(relation => !state.BlocksById.TryGetValue(relation.BlockId, out var block) ? null : new TagBlockRelationInfo
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
			});
	}

	/// <summary>
	/// Запрос информации о тегах и их источниках данных
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	public static IEnumerable<TagInfo> TagsInfoWithSources(this DatalakeDataState state)
	{
		var activeTags = state.Tags.Where(tag => !tag.IsDeleted);

		return activeTags
			.Where(tag => !tag.IsDeleted)
			.Select(tag =>
			{
				state.SourcesById.TryGetValue(tag.SourceId, out var source);
				return new TagInfo
				{
					Id = tag.Id,
					Guid = tag.GlobalGuid,
					Name = tag.Name,
					Description = tag.Description,
					Resolution = tag.Resolution,
					Type = tag.Type,
					Calculation = tag.Calculation,
					Formula = tag.Formula,
					Thresholds = tag.Thresholds,
					ThresholdSourceTag = !state.TagsById.TryGetValue(tag.ThresholdSourceTagId ?? 0, out var thresholdSourceTag) ? null : new TagAsInputInfo
					{
						Id = thresholdSourceTag.Id,
						Resolution = thresholdSourceTag.Resolution,
						Guid = thresholdSourceTag.GlobalGuid,
						Name = thresholdSourceTag.Name,
						Type = thresholdSourceTag.Type,
						BlockId = tag.SourceTagBlockId,
						SourceType = !state.SourcesById.TryGetValue(thresholdSourceTag.SourceId, out var thresholdSourceTagSource)
							? SourceType.NotSet
							: thresholdSourceTagSource.Type,
					},
					FormulaInputs = state.TagInputs
						.Where(relation => relation.TagId == tag.Id)
						.Join(activeTags, relation => relation.InputTagId, inputTag => inputTag.Id, (relation, inputTag) => new TagInputInfo
						{
							Id = inputTag.Id,
							Guid = inputTag.GlobalGuid,
							Name = inputTag.Name,
							VariableName = relation.VariableName,
							Type = inputTag.Type,
							Resolution = inputTag.Resolution,
							BlockId = relation.InputBlockId,
							SourceType = !state.SourcesById.TryGetValue(inputTag.SourceId, out var inputTagSource) ? SourceType.NotSet : inputTagSource.Type,
						})
						.ToArray(),
					IsScaling = tag.IsScaling,
					MaxEu = tag.MaxEu,
					MaxRaw = tag.MaxRaw,
					MinEu = tag.MinEu,
					MinRaw = tag.MinRaw,
					SourceId = tag.SourceId,
					SourceItem = tag.SourceItem,
					SourceType = source != null ? source.Type : SourceType.NotSet,
					SourceName = source != null ? source.Name : "Unknown",
					SourceTag = !state.TagsById.TryGetValue(tag.SourceTagId ?? 0, out var sourceTag) ? null : new TagAsInputInfo
					{
						Id = sourceTag.Id,
						Resolution = sourceTag.Resolution,
						Guid = sourceTag.GlobalGuid,
						Name = sourceTag.Name,
						Type = sourceTag.Type,
						BlockId = tag.SourceTagBlockId,
						SourceType = !state.SourcesById.TryGetValue(sourceTag.SourceId, out var sourceTagSource) ? SourceType.NotSet : sourceTagSource.Type,
					},
					Aggregation = tag.Aggregation,
					AggregationPeriod = tag.AggregationPeriod,
				};
			});
	}
}
