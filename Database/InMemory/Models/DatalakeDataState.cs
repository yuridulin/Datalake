using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;
using System.Collections.Immutable;

namespace Datalake.Database.InMemory.Models;

#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

public record class DatalakeDataState
{
	public long Version { get; private set; }

	// Таблицы

	public required ImmutableList<AccessRights> AccessRights { get; init; }

	public required ImmutableList<Block> Blocks { get; init; }

	public required ImmutableList<BlockProperty> BlockProperties { get; init; }

	public required ImmutableList<BlockTag> BlockTags { get; init; }

	public required ImmutableList<Source> Sources { get; init; }

	public required Settings Settings { get; init; }

	public required ImmutableList<Tag> Tags { get; init; }

	public required ImmutableList<TagInput> TagInputs { get; init; }

	public required ImmutableList<User> Users { get; init; }

	public required ImmutableList<UserGroup> UserGroups { get; init; }

	public required ImmutableList<UserGroupRelation> UserGroupRelations { get; init; }

	// Словари

	public void InitDictionaries()
	{
		BlocksById = Blocks.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		SourcesById = Sources.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		TagsByGuid = Tags.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.GlobalGuid);
		TagsById = Tags.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		UsersByGuid = Users.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Guid);
		UserGroupsByGuid = UserGroups.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Guid);

		var self = this;

		CachesTags = Tags
			.Select(tag => new TagCacheInfo
			{
				Id = tag.Id,
				Guid = tag.GlobalGuid,
				Name = tag.Name,
				Type = tag.Type,
				SourceId = tag.SourceId,
				SourceType = self.SourcesById.TryGetValue(tag.SourceId, out var source) ? source.Type : SourceType.NotSet,
				Frequency = tag.Frequency,
				ScalingCoefficient = tag.IsScaling
					? ((tag.MaxEu - tag.MinEu) / (tag.MaxRaw - tag.MinRaw))
					: 1,
				IsDeleted = tag.IsDeleted,
			})
			.ToImmutableList();

		CachedTagsById = CachesTags.ToImmutableDictionary(x => x.Id);
		CachedTagsByGuid = CachesTags.ToImmutableDictionary(x => x.Guid);

		Version = DateTime.UtcNow.Ticks;
	}

	public ImmutableDictionary<int, Block> BlocksById { get; private set; } = ImmutableDictionary<int, Block>.Empty;

	public ImmutableDictionary<int, Source> SourcesById { get; private set; } = ImmutableDictionary<int, Source>.Empty;

	public ImmutableDictionary<Guid, Tag> TagsByGuid { get; private set; } = ImmutableDictionary<Guid, Tag>.Empty;

	public ImmutableDictionary<int, Tag> TagsById { get; private set; } = ImmutableDictionary<int, Tag>.Empty;

	public ImmutableDictionary<Guid, User> UsersByGuid { get; private set; } = ImmutableDictionary<Guid, User>.Empty;

	public ImmutableDictionary<Guid, UserGroup> UserGroupsByGuid { get; private set; } = ImmutableDictionary<Guid, UserGroup>.Empty;

	public ImmutableList<TagCacheInfo> CachesTags { get; private set; } = ImmutableList<TagCacheInfo>.Empty;

	public ImmutableDictionary<int, TagCacheInfo> CachedTagsById { get; private set; } = ImmutableDictionary<int, TagCacheInfo>.Empty;

	public ImmutableDictionary<Guid, TagCacheInfo> CachedTagsByGuid { get; private set; } = ImmutableDictionary<Guid, TagCacheInfo>.Empty;
}

#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена