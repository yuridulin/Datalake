using Datalake.Database.InMemory.Models;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;

namespace Datalake.Database.Functions;

/// <summary></summary>
public static class AccessBuild
{
	private static DatalakeAccessStateMeta ComputeMeta(DatalakeDataState state)
	{
		DatalakeAccessStateMeta meta = new();

		foreach (var group in state.UserGroups)
		{
			var idWithParents = new List<Guid>();
			Guid? current = group.Guid;
			while (current.HasValue && state.UserGroupsByGuid.TryGetValue(current.Value, out var currentGroup))
			{
				idWithParents.Add(current.Value);
				current = currentGroup.ParentGuid;
			}
			meta.GroupsIdWithParents[group.Guid] = idWithParents;
		}

		foreach (var block in state.Blocks)
		{
			var idWithParents = new List<int>();
			int? current = block.Id;
			while (current.HasValue && state.BlocksById.TryGetValue(current.Value, out var currentBlock))
			{
				idWithParents.Add(current.Value);
				current = currentBlock.ParentId;
			}
			meta.BlocksIdWithParents[block.Id] = idWithParents;
		}

		var tagBlocksTemp = new Dictionary<int, List<int>>();
		foreach (var tr in state.BlockTags)
		{
			if (!tr.TagId.HasValue)
				continue;

			int tagId = tr.TagId.Value;
			if (!tagBlocksTemp.TryGetValue(tagId, out var blockList))
			{
				blockList = new List<int>();
				tagBlocksTemp[tagId] = blockList;
			}
			blockList.Add(tr.BlockId);
		}

		foreach (var kvp in tagBlocksTemp)
		{
			meta.TagBlocksRelation[kvp.Key] = kvp.Value;
		}

		foreach (var ur in state.UserGroupRelations)
		{
			if (!meta.UserGroupsRelation.TryGetValue(ur.UserGuid, out var groupDict))
			{
				groupDict = new Dictionary<Guid, AccessRuleInfo>();
				meta.UserGroupsRelation[ur.UserGuid] = groupDict;
			}
			groupDict[ur.UserGroupGuid] = new(ur.Id, ur.AccessType);
		}

		return meta;
	}

	private static Dictionary<Guid, UserAuthInfo> ComputeUsers(
		DatalakeDataState state,
		DatalakeAccessStateMeta meta,
		GroupTransfer[] computedGroups)
	{
		var userRights = state.Users
			.Select(user =>
			{
				// получаем базовое правило доступа пользователя, такое есть у каждого пользователя
				var globalRule = meta.UserGlobalRights.TryGetValue(user.Guid, out var gr) ? gr : meta.DefaultRule;

				// объявляем итоговый объект данных
				var auth = new UserAuthInfo
				{
					Guid = user.Guid,
					FullName = user.FullName ?? user.Login ?? string.Empty,
					Token = string.Empty,
					RootRule = globalRule,
				};

				// если у пользователя уже полный доступ, проверять остальное незачем
				if (globalRule.Access == AccessType.Admin)
				{
					foreach (var group in state.UserGroups)
						auth.Groups[group.Guid] = globalRule;

					foreach (var source in state.Sources)
						auth.Sources[source.Id] = globalRule;

					foreach (var block in state.Blocks)
						auth.Blocks[block.Id] = globalRule;

					foreach (var tag in state.Tags)
						auth.Tags[tag.Id] = globalRule;

					return auth;
				}

				// получаем прямые правила для объектов
				meta.UserBlockRights.TryGetValue(user.Guid, out var directAccessToBlock);
				meta.UserSourceRights.TryGetValue(user.Guid, out var directAccessToSource);
				meta.UserTagRights.TryGetValue(user.Guid, out var directAccessToTags);

				// вычисляем разрешения на доступ к группам
				List<GroupTransfer> groupsWithAccess = [];
				Dictionary<Guid, AccessRuleInfo> userGroups = [];
				foreach (var group in computedGroups)
				{
					var chosenRule = globalRule;

					if (meta.UserGroupsRelation.TryGetValue(user.Guid, out var groupsRelations))
					{
						foreach (var guid in group.IdWithParents)
						{
							if (groupsRelations.TryGetValue(guid, out var directRule) && directRule.Access > chosenRule.Access)
								chosenRule = directRule;
						}
					}

					userGroups[group.Guid] = chosenRule;
				}

				foreach (var block in state.Blocks)
				{
					var chosenRule = globalRule;

					if (directAccessToBlock != null)
					{
						foreach (var id in meta.BlocksIdWithParents[block.Id])
						{
							if (directAccessToBlock.TryGetValue(id, out var directRule) && directRule.Access > chosenRule.Access)
								chosenRule = directRule;
						}
					}

					foreach (var groupMap in groupsWithAccess)
					{
						if (groupMap.Blocks.TryGetValue(block.Id, out var groupToBlock) && groupToBlock.Access > chosenRule.Access)
							chosenRule = groupToBlock;
					}

					auth.Blocks[block.Id] = chosenRule;
				}

				foreach (var source in state.Sources)
				{
					var chosenRule = globalRule;

					if (directAccessToSource != null)
					{
						if (directAccessToSource.TryGetValue(source.Id, out var directRule)
							&& directRule.Access > chosenRule.Access)
							chosenRule = directRule;
					}

					foreach (var groupMap in groupsWithAccess)
					{
						if (groupMap.Sources.TryGetValue(source.Id, out var rule) && rule.Access > chosenRule.Access)
							chosenRule = rule;
					}

					auth.Sources[source.Id] = chosenRule;
				}

				foreach (var tag in state.Tags)
				{
					var chosenRule = globalRule;

					if (directAccessToTags != null)
					{
						if (directAccessToTags.TryGetValue(tag.Id, out var directRule) && directRule.Access > chosenRule.Access)
							chosenRule = directRule;
					}

					if (meta.TagBlocksRelation.TryGetValue(tag.Id, out var tagBlocks))
					{
						foreach (var tagBlockId in tagBlocks)
						{
							if (auth.Blocks.TryGetValue(tagBlockId, out var rule) && rule.Access > chosenRule.Access)
								chosenRule = rule;
						}
					}

					foreach (var groupMap in groupsWithAccess)
					{
						if (groupMap.Tags.TryGetValue(tag.Id, out var rule) && rule.Access > chosenRule.Access)
							chosenRule = rule;
					}

					auth.Tags[tag.Id] = chosenRule;
				}

				return auth;
			})
			.ToDictionary(x => x.Guid);

		return userRights;
	}

	/// <summary>
	/// Расчет прав доступа всех пользователей ко всем объектам
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	/// <returns>Новое состояние прав доступа</returns>
	public static DatalakeAccessState ComputateRightsFromState(DatalakeDataState state)
	{
		ComputeMeta(state);
		//var userGroupsRights = ComputeUserGroups(state, meta);
		//var userRights = ComputeUsers(state, meta, userGroupsRights);
		//var accessState = new DatalakeAccessState(userRights);

		var accessState = new DatalakeAccessState();

		return accessState;
	}

	private class DatalakeAccessStateMeta
	{
		public Dictionary<Guid, List<Guid>> GroupsIdWithParents { get; set; } = [];
		public Dictionary<int, List<int>> BlocksIdWithParents { get; set; } = [];
		public Dictionary<Guid, AccessRuleInfo> GroupGlobalRights { get; set; } = [];
		public Dictionary<Guid, Dictionary<int, AccessRuleInfo>> GroupBlockRights { get; set; } = [];
		public Dictionary<Guid, Dictionary<int, AccessRuleInfo>> GroupSourceRights { get; set; } = [];
		public Dictionary<Guid, Dictionary<int, AccessRuleInfo>> GroupTagRights { get; set; } = [];
		public Dictionary<Guid, AccessRuleInfo> UserGlobalRights { get; set; } = [];
		public Dictionary<Guid, Dictionary<int, AccessRuleInfo>> UserBlockRights { get; set; } = [];
		public Dictionary<Guid, Dictionary<int, AccessRuleInfo>> UserSourceRights { get; set; } = [];
		public Dictionary<Guid, Dictionary<int, AccessRuleInfo>> UserTagRights { get; set; } = [];
		public Dictionary<int, List<int>> TagBlocksRelation { get; set; } = [];
		public Dictionary<Guid, Dictionary<Guid, AccessRuleInfo>> UserGroupsRelation { get; set; } = [];
		public AccessRuleInfo DefaultRule { get; set; } = new(0, AccessType.NotSet);
	}

	private class GroupTransfer
	{
		public Guid Guid { get; set; }
		public required AccessRuleInfo GlobalRule { get; set; }
		public List<Guid> IdWithParents { get; set; } = [];
		public Dictionary<int, AccessRuleInfo> Sources { get; set; } = [];
		public Dictionary<int, AccessRuleInfo> Blocks { get; set; } = [];
		public Dictionary<int, AccessRuleInfo> Tags { get; set; } = [];
	}
}
