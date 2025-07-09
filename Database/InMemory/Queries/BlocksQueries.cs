using Datalake.Database.InMemory.Models;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.PublicApi.Models.Users;
using LinqToDB;
using static Datalake.PublicApi.Models.Blocks.BlockFullInfo;

namespace Datalake.Database.InMemory.Queries;

/// <summary>
/// Запросы, связанные с блоками
/// </summary>
public static class BlocksQueries
{
	/// <summary>
	/// Запрос информации о блоках со списками тегов
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	public static IEnumerable<BlockWithTagsInfo> BlocksInfoWithTags(this DatalakeDataState state)
	{
		return state.Blocks
			.Where(block => !block.IsDeleted)
			.Select(block => new BlockWithTagsInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				ParentId = block.ParentId,
				Tags = state.BlockTags
					.Where(relation => relation.BlockId == block.Id)
					.Join(state.Tags, relation => relation.TagId, tag => tag.Id, (relation, tag) => new BlockNestedTagInfo
					{
						Id = tag.Id,
						Name = tag.Name,
						Guid = tag.GlobalGuid,
						RelationId = relation.Id,
						RelationType = relation.Relation,
						LocalName = relation.Name ?? tag.Name,
						Type = tag.Type,
						Frequency = tag.Frequency,
						SourceId = tag.SourceId,
						SourceType = state.SourcesById.TryGetValue(tag.SourceId, out var source) ? source.Type : SourceType.NotSet,
					})
					.ToArray(),
			});
	}

	/// <summary>
	/// Запрос информации о конкретном блоке со всеми его отношениями к другим объектам
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	/// <param name="blockId">Идентификатор блока</param>
	public static BlockFullInfo BlockInfoWithParentsAndTags(this DatalakeDataState state, int blockId)
	{
		if (!state.BlocksById.TryGetValue(blockId, out var block))
			throw new NotFoundException("блок #" + blockId);

		var adults = new List<BlockTreeInfo>();

		int? currentParentId = block.ParentId;
		do
		{
			if (state.BlocksById.TryGetValue(currentParentId ?? 0, out var adult))
			{
				adults.Add(new BlockTreeInfo
				{
					Id = adult.Id,
					Guid = adult.GlobalId,
					Name = adult.Name,
					ParentId = adult.ParentId,
				});
				currentParentId = adult.ParentId;
			}
		}
		while ((currentParentId ?? 0) != 0);

		var blockInfo = new BlockFullInfo
		{
			Id = block.Id,
			Guid = block.GlobalId,
			Name = block.Name,
			Description = block.Description,
			ParentId = block.ParentId,
			Parent = !state.BlocksById.TryGetValue(block.ParentId ?? 0, out var parent) ? null : new BlockParentInfo
			{
				Id = parent.Id,
				Name = parent.Name,
			},
			Adults = adults
				.ToArray(),
			Children = state.Blocks
				.Where(child => child.ParentId == block.Id)
				.Select(child => new BlockChildInfo
				{
					Id = child.Id,
					Name = child.Name,
				})
				.ToArray(),
			Properties = state.BlockProperties
				.Where(property => property.BlockId == block.Id)
				.Select(property => new BlockPropertyInfo
				{
					Id = property.Id,
					Name = property.Name,
					Type = property.Type,
					Value = property.Value,
				})
				.ToArray(),
			Tags = state.BlockTags
				.Where(relation => relation.BlockId == block.Id)
				.Select(relation => !state.TagsById.TryGetValue(relation.TagId ?? 0, out var tag) ? null : new BlockNestedTagInfo
				{
					Id = tag.Id,
					Name = tag.Name,
					Guid = tag.GlobalGuid,
					RelationId = relation.Id,
					RelationType = relation.Relation,
					LocalName = relation.Name ?? tag.Name,
					Type = tag.Type,
					Frequency = tag.Frequency,
					SourceId = tag.SourceId,
					SourceType = state.SourcesById.TryGetValue(tag.SourceId, out var source) ? source.Type : SourceType.NotSet,
				})
				.Where(x => x != null)
				.ToArray()!,
			AccessRights = state.AccessRights
				.Where(rule => rule.BlockId == block.Id)
				.Select(rule => new AccessRightsForObjectInfo
				{
					Id = rule.Id,
					IsGlobal = rule.IsGlobal,
					AccessType = rule.AccessType,
					User = !state.UsersByGuid.TryGetValue(rule.UserGuid ?? Guid.Empty, out var user) ? null : new UserSimpleInfo
					{
						Guid = user.Guid,
						FullName = user.FullName ?? string.Empty,
					},
					UserGroup = !state.UserGroupsByGuid.TryGetValue(rule.UserGroupGuid ?? Guid.Empty, out var usergroup) ? null : new UserGroupSimpleInfo
					{
						Guid = usergroup.Guid,
						Name = usergroup.Name,
					},
				})
				.ToArray(),
		};

		return blockInfo;
	}
}
