using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;
using System.Collections.Immutable;

namespace Datalake.Database.InMemory.Models;

#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

public struct DatalakeDataState
{
	public long Version { get; private set; }

	// Таблицы

	public ImmutableList<AccessRights> AccessRights { get; init; }

	public ImmutableList<Block> Blocks { get; init; }

	public ImmutableList<BlockProperty> BlockProperties { get; init; }

	public ImmutableList<BlockTag> BlockTags { get; init; }

	public ImmutableList<Source> Sources { get; init; }

	public Settings Settings { get; init; }

	public ImmutableList<Tag> Tags { get; init; }

	public ImmutableList<TagInput> TagInputs { get; init; }

	public ImmutableList<User> Users { get; init; }

	public ImmutableList<UserGroup> UserGroups { get; init; }

	public ImmutableList<UserGroupRelation> UserGroupRelations { get; init; }

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

	public ImmutableDictionary<int, Block> BlocksById { get; private set; }

	public ImmutableDictionary<int, Source> SourcesById { get; private set; }

	public ImmutableDictionary<Guid, Tag> TagsByGuid { get; private set; }

	public ImmutableDictionary<int, Tag> TagsById { get; private set; }

	public ImmutableDictionary<Guid, User> UsersByGuid { get; private set; }

	public ImmutableDictionary<Guid, UserGroup> UserGroupsByGuid { get; private set; }

	public ImmutableList<TagCacheInfo> CachesTags { get; private set; }

	public ImmutableDictionary<int, TagCacheInfo> CachedTagsById { get; private set; }

	public ImmutableDictionary<Guid, TagCacheInfo> CachedTagsByGuid { get; private set; }
}

#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена