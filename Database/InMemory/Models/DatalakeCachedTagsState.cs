using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;
using System.Collections.Immutable;

namespace Datalake.Database.InMemory.Models;

#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

public record class DatalakeCachedTagsState
{
	/// <summary>
	/// Cписок информации о тегах для записи
	/// </summary>
	public ImmutableList<TagCacheInfo> CachedTags { get; init; }

	/// <summary>
	/// Cписок информации о тегах для записи, сопоставленный с локальными идентификаторами тегов
	/// </summary>
	public ImmutableDictionary<int, TagCacheInfo> CachedTagsById { get; init; }

	/// <summary>
	/// Cписок информации о тегах для записи, сопоставленный с глобальными идентификаторами тегов
	/// </summary>
	public ImmutableDictionary<Guid, TagCacheInfo> CachedTagsByGuid { get; init; }

	/// <summary>
	/// Создание кэша по тегам для записи значений
	/// </summary>
	/// <param name="dataState">Текущий снимок данных БД</param>
	public DatalakeCachedTagsState(DatalakeDataState dataState)
	{
		CachedTags = dataState.Tags
			.Select(tag => new TagCacheInfo
			{
				Id = tag.Id,
				Guid = tag.GlobalGuid,
				Name = tag.Name,
				Type = tag.Type,
				SourceId = tag.SourceId,
				SourceType = dataState.SourcesById.TryGetValue(tag.SourceId, out var source) ? source.Type : SourceType.NotSet,
				Resolution = tag.Resolution,
				ScalingCoefficient = tag.IsScaling
						? ((tag.MaxEu - tag.MinEu) / (tag.MaxRaw - tag.MinRaw))
						: 1,
				IsDeleted = tag.IsDeleted,
			})
			.ToImmutableList();

		CachedTagsById = CachedTags.ToImmutableDictionary(x => x.Id);
		CachedTagsByGuid = CachedTags.ToImmutableDictionary(x => x.Guid);
	}

	/// <summary>
	/// Создание пустого кэша по тегам для записи значений
	/// </summary>
	public DatalakeCachedTagsState()
	{
		CachedTags = [];

		CachedTagsById = ImmutableDictionary<int, TagCacheInfo>.Empty;
		CachedTagsByGuid = ImmutableDictionary<Guid, TagCacheInfo>.Empty;
	}
}

#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена