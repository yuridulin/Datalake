using Datalake.Database.Constants;
using Datalake.Database.InMemory.Models;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.Database.InMemory.Queries;

/// <summary>
/// Запросы, связанные с источниками данных
/// </summary>
public static class SourceQueries
{
	/// <summary>
	/// Запрос информации о источниках без связей
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	/// <param name="withCustom">Включать ли системные источники в запрос</param>
	public static IEnumerable<SourceInfo> SourcesInfo(this DatalakeDataState state, bool withCustom = false)
	{
		return state.Sources
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
	}

	/// <summary>
	/// Запрос информации о источниках вместе со списками зависящих тегов
	/// </summary>
	public static IEnumerable<SourceWithTagsInfo> SourcesInfoWithTags(this DatalakeDataState state)
	{
		return state.Sources
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
						Frequency = tag.Frequency,
						SourceType = source.Type,
						Aggregation = tag.Aggregation,
						AggregationPeriod = tag.AggregationPeriod,
					})
					.ToArray(),
			});
	}

	/// <summary>
	/// Запрос информации о источниках вместе со списками зависящих тегов
	/// </summary>
	public static IEnumerable<SourceWithTagsInfo> SourcesInfoWithTagsAndSourceTags(this DatalakeDataState state)
	{
		return state.Sources
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
						Formula = tag.Formula,
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
						Frequency = tag.Frequency,
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
	}
}
