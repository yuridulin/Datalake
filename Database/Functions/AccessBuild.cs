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

	private static GroupTransfer[] ComputeUserGroups(
		DatalakeDataState state,
		DatalakeAccessStateMeta meta)
	{
		return state.UserGroups
			.Select(group =>
			{
				// заранее вычисляем уровни доступа каждой группы к каждому объекту, чтобы облегчить дальнейший расчет по пользователям
				GroupTransfer auth = new()
				{
					Guid = group.Guid,
					IdWithParents = meta.GroupsIdWithParents[group.Guid],
				};

				// получаем базовое правило группы, такое есть у каждой группы
				var globalRule = meta.GroupGlobalRights.TryGetValue(group.Guid, out var rr) ? rr : meta.DefaultRule;

				// если базовый уровень доступа уже максимальный, незачем проверять остальное
				// если нет, начинаем искать для каждого объекта правило с наибольшим уровнем доступа
				if (globalRule.AccessType == AccessType.Admin)
				{
					foreach (var source in state.Sources)
						auth.Sources[source.Id] = globalRule;

					foreach (var block in state.Blocks)
						auth.Blocks[block.Id] = globalRule;

					foreach (var tag in state.Tags)
						auth.Tags[tag.Id] = globalRule;

					return auth;
				}

				// проверяем прямые правила группы на источник
				if (meta.GroupSourceRights.TryGetValue(group.Guid, out var directAccessToSource))
				{
					foreach (var source in state.Sources)
					{
						// если уровень доступа на прямом правиле на источник выше, чем на базовом, то берем его
						if (directAccessToSource.TryGetValue(source.Id, out var directRule) &&
							directRule.AccessType > globalRule.AccessType)
						{
							auth.Sources[source.Id] = directRule;
						}
						else
						{
							auth.Sources[source.Id] = globalRule;
						}
					}
				}
				// если прямых правил нет, базовое правило группы остается в силе
				else
				{
					foreach (var source in state.Sources)
					{
						auth.Sources[source.Id] = globalRule;
					}
				}

				// проверяем прямые правила группы на блок тегов
				if (meta.GroupBlockRights.TryGetValue(group.Guid, out var directAccessToBlock))
				{
					foreach (var block in state.Blocks)
					{
						// берем базовое правило группы за основу
						var chosenRule = globalRule;

						// получаем ранее вычисленную цепочку наследования
						var blockIdWithParents = meta.BlocksIdWithParents[block.Id];

						// поднимаемся по иерархии в поисках правила выше
						foreach (var id in blockIdWithParents)
						{
							if (directAccessToBlock.TryGetValue(id, out var candidateRule) &&
								candidateRule.AccessType > chosenRule.AccessType)
							{
								chosenRule = candidateRule;
							}
						}

						// сохраняем итог
						auth.Blocks[block.Id] = chosenRule;
					}
				}
				// если прямых правил нет, базовое правило группы остается в силе
				else
				{
					foreach (var block in state.Blocks)
					{
						auth.Blocks[block.Id] = globalRule;
					}
				}

				// проверяем прямые правила группы на каждый отдельный тег
				meta.GroupTagRights.TryGetValue(group.Guid, out var directAccessToTag);
				foreach (var tag in state.Tags)
				{
					// берем базовое правило группы за основу
					var chosenRule = globalRule;

					// проверка прямого правила группы на тег, при доступе выше базового запоминаем его
					if (directAccessToTag != null &&
						directAccessToTag.TryGetValue(tag.Id, out var directRule) &&
						directRule.AccessType > globalRule.AccessType)
					{
						chosenRule = directRule;
					}

					// проверяем блоки, в которых есть тег
					if (meta.TagBlocksRelation.TryGetValue(tag.Id, out var blockIds))
					{
						foreach (var blockId in blockIds)
						{
							// если доступ к какому-то блоку выше базового, это работает и на его теги
							if (auth.Blocks.TryGetValue(blockId, out var blockRule) &&
								blockRule.AccessType > chosenRule.AccessType)
							{
								chosenRule = blockRule;
							}
						}
					}

					// сохраняем результат
					auth.Tags[tag.Id] = chosenRule;
				}

				// возвращаем рассчитанный доступ группы
				return auth;
			})
			.ToArray();
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
					GlobalAccessType = globalRule.AccessType,
				};

				// если у пользователя уже полный доступ, проверять остальное незачем
				if (globalRule.AccessType == AccessType.Admin)
				{
					foreach (var group in state.UserGroups)
						auth.Groups[group.Guid] = globalRule;

					foreach (var source in state.Sources)
						auth.Sources[source.Id] = globalRule;

					foreach (var block in state.Blocks)
						auth.Blocks[block.Id] = globalRule;

					foreach (var tag in state.Tags)
						auth.Tags[tag.GlobalGuid] = globalRule;

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
							if (groupsRelations.TryGetValue(guid, out var directRule) && directRule.AccessType > chosenRule.AccessType)
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
							if (directAccessToBlock.TryGetValue(id, out var directRule) && directRule.AccessType > chosenRule.AccessType)
								chosenRule = directRule;
						}
					}

					foreach (var groupMap in groupsWithAccess)
					{
						if (groupMap.Blocks.TryGetValue(block.Id, out var groupToBlock) && groupToBlock.AccessType > chosenRule.AccessType)
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
							&& directRule.AccessType > chosenRule.AccessType)
							chosenRule = directRule;
					}

					foreach (var groupMap in groupsWithAccess)
					{
						if (groupMap.Sources.TryGetValue(source.Id, out var rule) && rule.AccessType > chosenRule.AccessType)
							chosenRule = rule;
					}

					auth.Sources[source.Id] = chosenRule;
				}

				foreach (var tag in state.Tags)
				{
					var chosenRule = globalRule;

					if (directAccessToTags != null)
					{
						if (directAccessToTags.TryGetValue(tag.Id, out var directRule) && directRule.AccessType > chosenRule.AccessType)
							chosenRule = directRule;
					}

					if (meta.TagBlocksRelation.TryGetValue(tag.Id, out var tagBlocks))
					{
						foreach (var tagBlockId in tagBlocks)
						{
							if (auth.Blocks.TryGetValue(tagBlockId, out var rule) && rule.AccessType > chosenRule.AccessType)
								chosenRule = rule;
						}
					}

					foreach (var groupMap in groupsWithAccess)
					{
						if (groupMap.Tags.TryGetValue(tag.Id, out var rule) && rule.AccessType > chosenRule.AccessType)
							chosenRule = rule;
					}

					auth.Tags[tag.GlobalGuid] = chosenRule;
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
		var meta = ComputeMeta(state);
		var userGroupsRights = ComputeUserGroups(state, meta);
		var userRights = ComputeUsers(state, meta, userGroupsRights);
		var accessState = new DatalakeAccessState(userRights);

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
		public List<Guid> IdWithParents { get; set; } = [];
		public Dictionary<int, AccessRuleInfo> Sources { get; set; } = [];
		public Dictionary<int, AccessRuleInfo> Blocks { get; set; } = [];
		public Dictionary<int, AccessRuleInfo> Tags { get; set; } = [];
	}
}
