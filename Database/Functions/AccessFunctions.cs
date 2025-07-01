using Datalake.Database.InMemory.Models;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;

namespace Datalake.Database.Functions;

/// <summary>
/// Функции предварительного расчета прав доступа для пользователей
/// </summary>
public static class AccessFunctions
{
	/// <summary>
	/// Расчет прав доступа по пользователям на основании текущих данных
	/// </summary>
	/// <param name="state">Состояние с текущими данными</param>
	/// <returns>Состояние актуальных прав доступа</returns>
	public static DatalakeAccessState ComputeAccess(DatalakeDataState state)
	{
		// Оптимизация: ленивая инициализация словарей правил
		var userGlobalRules = new Dictionary<Guid, AccessRuleInfo>();
		var groupGlobalRules = new Dictionary<Guid, AccessRuleInfo>();

		var userRulesToSources = new Dictionary<Guid, Dictionary<int, AccessRuleInfo>>();
		var userRulesToBlocks = new Dictionary<Guid, Dictionary<int, AccessRuleInfo>>();
		var userRulesToTags = new Dictionary<Guid, Dictionary<int, AccessRuleInfo>>();

		var groupRulesToSources = new Dictionary<Guid, Dictionary<int, AccessRuleInfo>>();
		var groupRulesToBlocks = new Dictionary<Guid, Dictionary<int, AccessRuleInfo>>();
		var groupRulesToTags = new Dictionary<Guid, Dictionary<int, AccessRuleInfo>>();

		foreach (var r in state.AccessRights)
		{
			var rule = new AccessRuleInfo(r.Id, r.AccessType);

			if (r.UserGuid.HasValue)
			{
				var userId = r.UserGuid.Value;
				if (r.IsGlobal)
				{
					userGlobalRules[userId] = rule;
				}
				else if (r.SourceId.HasValue)
				{
					AddToMap(userRulesToSources, userId, r.SourceId.Value, rule);
				}
				else if (r.BlockId.HasValue)
				{
					AddToMap(userRulesToBlocks, userId, r.BlockId.Value, rule);
				}
				else if (r.TagId.HasValue)
				{
					AddToMap(userRulesToTags, userId, r.TagId.Value, rule);
				}
			}
			else if (r.UserGroupGuid.HasValue)
			{
				var groupId = r.UserGroupGuid.Value;
				if (r.IsGlobal)
				{
					groupGlobalRules[groupId] = rule;
				}
				else if (r.SourceId.HasValue)
				{
					AddToMap(groupRulesToSources, groupId, r.SourceId.Value, rule);
				}
				else if (r.BlockId.HasValue)
				{
					AddToMap(groupRulesToBlocks, groupId, r.BlockId.Value, rule);
				}
				else if (r.TagId.HasValue)
				{
					AddToMap(groupRulesToTags, groupId, r.TagId.Value, rule);
				}
			}
		}

		// Оптимизация: предварительный расчет иерархии блоков
		var blockParentMap = state.Blocks.ToDictionary(b => b.Id, b => b.ParentId);
		var blockAncestors = new Dictionary<int, HashSet<int>>();
		foreach (var block in state.Blocks)
		{
			var ancestors = new HashSet<int>();
			int? current = block.Id;
			while (current.HasValue)
			{
				ancestors.Add(current.Value);
				if (blockParentMap.TryGetValue(current.Value, out var parentId) && parentId.HasValue)
					current = parentId.Value;
				else
					current = null;
			}
			blockAncestors[block.Id] = ancestors;
		}

		// Оптимизация: предварительный расчет иерархии групп
		var groupParentMap = state.UserGroups.ToDictionary(g => g.Guid, g => g.ParentGuid);
		var groupAncestors = new Dictionary<Guid, HashSet<Guid>>();
		foreach (var group in state.UserGroups)
		{
			var ancestors = new HashSet<Guid>();
			Guid? current = group.Guid;
			while (current.HasValue)
			{
				ancestors.Add(current.Value);
				if (groupParentMap.TryGetValue(current.Value, out var parentGuid) && parentGuid.HasValue)
					current = parentGuid.Value;
				else
					current = null;
			}
			groupAncestors[group.Guid] = ancestors;
		}

		// Оптимизация: связь пользователь-группы
		var directUserGroupRules = new Dictionary<Guid, Dictionary<Guid, AccessRuleInfo>>();
		var userGroups = new Dictionary<Guid, HashSet<Guid>>();
		foreach (var relation in state.UserGroupRelations)
		{
			if (!userGroups.TryGetValue(relation.UserGuid, out var groups))
			{
				groups = new HashSet<Guid>();
				userGroups[relation.UserGuid] = groups;
			}

			if (groupAncestors.TryGetValue(relation.UserGroupGuid, out var ancestors))
			{
				groups.UnionWith(ancestors);
			}

			var userId = relation.UserGuid;
			var groupId = relation.UserGroupGuid;
			var rule = new AccessRuleInfo(relation.Id, relation.AccessType);

			if (!directUserGroupRules.TryGetValue(userId, out var userRules))
			{
				userRules = new Dictionary<Guid, AccessRuleInfo>();
				directUserGroupRules[userId] = userRules;
			}

			// Обновляем правило если найден более высокий уровень доступа
			if (!userRules.TryGetValue(groupId, out var existing) ||
					rule.Access > existing.Access)
			{
				userRules[groupId] = rule;
			}
		}

		// Оптимизация: связь тег-блоки
		var blocksByTag = new Dictionary<int, HashSet<int>>();
		foreach (var bt in state.BlockTags)
		{
			if (!bt.TagId.HasValue)
				continue;

			int tagId = bt.TagId.Value;
			if (!blocksByTag.TryGetValue(tagId, out var blockIds))
			{
				blockIds = new HashSet<int>();
				blocksByTag[tagId] = blockIds;
			}
			blockIds.Add(bt.BlockId);
		}

		// Расчет прав для пользователей
		var usersAccess = new Dictionary<Guid, UserAuthInfo>();
		foreach (var user in state.Users)
		{
			Guid userGuid = user.Guid;
			var access = new UserAuthInfo
			{
				Guid = userGuid,
				FullName = user.FullName ?? user.Login ?? string.Empty,
				RootRule = AccessRuleInfo.Default,
				Token = string.Empty
			};

			AccessRuleInfo globalRule = AccessRuleInfo.Default;

			// Глобальные правила пользователя
			if (userGlobalRules.TryGetValue(userGuid, out var userGlobalRule))
			{
				globalRule = userGlobalRule;
			}

			// Оптимизация: объединение групповых правил
			Dictionary<int, AccessRuleInfo> groupSourceRules = null!;
			Dictionary<int, AccessRuleInfo> groupBlockRules = null!;
			Dictionary<int, AccessRuleInfo> groupTagRules = null!;

			if (userGroups.TryGetValue(userGuid, out var userGroupSet))
			{
				// Глобальные правила групп
				foreach (var groupGuid in userGroupSet)
				{
					if (groupGlobalRules.TryGetValue(groupGuid, out var groupRule) &&
							groupRule.Access > globalRule.Access)
					{
						globalRule = groupRule;
					}
				}
				
				// Получаем прямые правила пользователя
				directUserGroupRules.TryGetValue(userGuid, out var userDirectGroupRules);

				foreach (var groupGuid in userGroupSet)
				{
					// Проверяем наличие прямого правила
					if (userDirectGroupRules != null &&
							userDirectGroupRules.TryGetValue(groupGuid, out var rule))
					{
						access.Groups[groupGuid] = rule;
					}
				}

				// Предварительное объединение групповых правил
				groupSourceRules = new Dictionary<int, AccessRuleInfo>();
				groupBlockRules = new Dictionary<int, AccessRuleInfo>();
				groupTagRules = new Dictionary<int, AccessRuleInfo>();

				foreach (var groupGuid in userGroupSet)
				{
					// Для источников
					if (groupRulesToSources.TryGetValue(groupGuid, out var rules))
					{
						foreach (var kv in rules)
						{
							if (!groupSourceRules.TryGetValue(kv.Key, out var current) ||
									kv.Value.Access > current.Access)
							{
								groupSourceRules[kv.Key] = kv.Value;
							}
						}
					}

					// Для блоков
					if (groupRulesToBlocks.TryGetValue(groupGuid, out rules))
					{
						foreach (var kv in rules)
						{
							if (!groupBlockRules.TryGetValue(kv.Key, out var current) ||
									kv.Value.Access > current.Access)
							{
								groupBlockRules[kv.Key] = kv.Value;
							}
						}
					}

					// Для тегов
					if (groupRulesToTags.TryGetValue(groupGuid, out rules))
					{
						foreach (var kv in rules)
						{
							if (!groupTagRules.TryGetValue(kv.Key, out var current) ||
									kv.Value.Access > current.Access)
							{
								groupTagRules[kv.Key] = kv.Value;
							}
						}
					}
				}
			}

			access.RootRule = globalRule;

			// Пропускаем расчет объектов для администраторов и заблокированных
			if (globalRule.Access is AccessType.Admin or AccessType.NoAccess)
			{
				usersAccess[userGuid] = access;
				continue;
			}

			// Получаем пользовательские правила
			userRulesToSources.TryGetValue(userGuid, out var userSourceRules);
			userRulesToBlocks.TryGetValue(userGuid, out var userBlockRules);
			userRulesToTags.TryGetValue(userGuid, out var userTagRules);

			// 1. Расчет прав для источников
			foreach (var source in state.Sources)
			{
				int sourceId = source.Id;
				var rule = globalRule;

				// Пользовательские правила
				if (userSourceRules != null &&
						userSourceRules.TryGetValue(sourceId, out var userRule))
				{
					rule = userRule;
				}

				// Групповые правила
				if (groupSourceRules != null &&
						groupSourceRules.TryGetValue(sourceId, out var groupRule) &&
						groupRule.Access > rule.Access)
				{
					rule = groupRule;
				}

				if (rule.Access > globalRule.Access)
				{
					access.Sources[sourceId] = rule;
				}
			}

			// 2. Расчет прав для блоков
			foreach (var block in state.Blocks)
			{
				int blockId = block.Id;
				var rule = globalRule;

				// Пользовательские правила для блока
				if (userBlockRules != null &&
						userBlockRules.TryGetValue(blockId, out var userRule))
				{
					rule = userRule;
				}

				// Групповые правила для блока
				if (groupBlockRules != null &&
						groupBlockRules.TryGetValue(blockId, out var groupRule) &&
						groupRule.Access > rule.Access)
				{
					rule = groupRule;
				}

				// Наследование от предков
				if (blockAncestors.TryGetValue(blockId, out var ancestors))
				{
					foreach (var ancestorId in ancestors)
					{
						// Пользовательские правила предка
						if (userBlockRules != null &&
								userBlockRules.TryGetValue(ancestorId, out userRule) &&
								userRule.Access > rule.Access)
						{
							rule = userRule;
						}

						// Групповые правила предка
						if (groupBlockRules != null &&
								groupBlockRules.TryGetValue(ancestorId, out groupRule) &&
								groupRule.Access > rule.Access)
						{
							rule = groupRule;
						}
					}
				}

				if (rule.Access > globalRule.Access)
				{
					access.Blocks[blockId] = rule;
				}
			}

			// 3. Расчет прав для тегов
			foreach (var tag in state.Tags)
			{
				int tagId = tag.Id;
				var rule = globalRule;

				// Прямые пользовательские правила
				if (userTagRules != null &&
						userTagRules.TryGetValue(tagId, out var userRule))
				{
					rule = userRule;
				}

				// Прямые групповые правила
				if (groupTagRules != null &&
						groupTagRules.TryGetValue(tagId, out var groupRule) &&
						groupRule.Access > rule.Access)
				{
					rule = groupRule;
				}

				// Правила через связанные блоки
				if (blocksByTag.TryGetValue(tagId, out var blockIds))
				{
					foreach (var blockId in blockIds)
					{
						if (access.Blocks.TryGetValue(blockId, out var blockRule) &&
								blockRule.Access > rule.Access)
						{
							rule = blockRule;
						}
					}
				}

				if (rule.Access > globalRule.Access)
				{
					access.Tags[tagId] = rule;
				}
			}

			usersAccess[userGuid] = access;
		}

		return new(usersAccess);
	}

	// Вспомогательный метод для добавления правил
	private static void AddToMap<TKey, TValue>(
		Dictionary<TKey, Dictionary<TValue, AccessRuleInfo>> map,
		TKey owner,
		TValue objId,
		AccessRuleInfo rule)
		where TKey : notnull
		where TValue : notnull
	{
		if (!map.TryGetValue(owner, out var innerMap))
		{
			innerMap = new Dictionary<TValue, AccessRuleInfo>();
			map[owner] = innerMap;
		}
		innerMap[objId] = rule;
	}
}
