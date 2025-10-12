using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Models;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Entities;
using System.Collections.Concurrent;

namespace Datalake.Inventory.Application.Services;

/// <summary>
/// Функции предварительного расчета прав доступа для пользователей
/// </summary>
[Singleton]
public class UserAccessCalculationService : IUserAccessCalculationService
{
	/// <summary>
	/// Расчет прав доступа по пользователям на основании текущих данных
	/// </summary>
	/// <param name="state">Состояние с текущими данными</param>
	/// <returns>Состояние актуальных прав доступа</returns>
	public UsersAccessDto CalculateAccess(IInventoryCacheState state)
	{
		// Предварительные вычисления
		var precomputed = PrecomputeStructures(state);

		// Оптимизация: ленивая инициализация словарей правил
		var hashSets = PrepareHashSets(state);

		// Параллельная обработка пользователей для больших наборов
		var usersAccess = new ConcurrentDictionary<Guid, UserAccessEntity>();

		Parallel.ForEach(state.Users, ParallelOptions, user =>
		{
			usersAccess[user.Key] = CalculateUserAccess(user.Value, state, precomputed, hashSets);
		});

		return new UsersAccessDto
		{
			Version = state.Version,
			UserAccessEntities = usersAccess.ToDictionary(x => x.Key, x => x.Value as IUserAccessEntity),
		};
	}

	private static HashSets PrepareHashSets(IInventoryCacheState state)
	{
		var userGlobalRules = new Dictionary<Guid, AccessRuleValue>();
		var groupGlobalRules = new Dictionary<Guid, AccessRuleValue>();

		var userRulesToSources = new Dictionary<Guid, Dictionary<int, AccessRuleValue>>();
		var userRulesToBlocks = new Dictionary<Guid, Dictionary<int, AccessRuleValue>>();
		var userRulesToTags = new Dictionary<Guid, Dictionary<int, AccessRuleValue>>();

		var groupRulesToSources = new Dictionary<Guid, Dictionary<int, AccessRuleValue>>();
		var groupRulesToBlocks = new Dictionary<Guid, Dictionary<int, AccessRuleValue>>();
		var groupRulesToTags = new Dictionary<Guid, Dictionary<int, AccessRuleValue>>();

		foreach (var r in state.AccessRules)
		{
			var rule = new AccessRuleValue(r.Id, r.AccessType);

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

		return new HashSets(
			userGlobalRules,
			groupGlobalRules,
			userRulesToSources,
			userRulesToBlocks,
			userRulesToTags,
			groupRulesToSources,
			groupRulesToBlocks,
			groupRulesToTags);
	}

	private static Precomputed PrecomputeStructures(IInventoryCacheState state)
	{
		// Оптимизация: предварительный расчет иерархии блоков
		var blockParentMap = state.Blocks.ToDictionary(b => b.Key, b => b.Value.ParentId);
		var blockAncestors = new Dictionary<int, HashSet<int>>();
		foreach (var block in state.Blocks)
		{
			var ancestors = new HashSet<int>();
			int? current = block.Key;
			while (current.HasValue)
			{
				ancestors.Add(current.Value);
				if (blockParentMap.TryGetValue(current.Value, out var parentId) && parentId.HasValue)
					current = parentId.Value;
				else
					current = null;
			}
			blockAncestors[block.Key] = ancestors;
		}

		// Оптимизация: предварительный расчет иерархии групп
		var groupParentMap = state.UserGroups.ToDictionary(g => g.Key, g => g.Value.ParentGuid);
		var groupAncestors = new Dictionary<Guid, HashSet<Guid>>();
		foreach (var group in state.UserGroups)
		{
			var ancestors = new HashSet<Guid>();
			Guid? current = group.Key;
			while (current.HasValue)
			{
				ancestors.Add(current.Value);
				if (groupParentMap.TryGetValue(current.Value, out var parentGuid) && parentGuid.HasValue)
					current = parentGuid.Value;
				else
					current = null;
			}
			groupAncestors[group.Key] = ancestors;
		}

		// Оптимизация: связь пользователь-группы
		var directUserGroupRules = new Dictionary<Guid, Dictionary<Guid, AccessRuleValue>>();
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
			var rule = new AccessRuleValue(relation.Id, relation.AccessType);

			if (!directUserGroupRules.TryGetValue(userId, out var userRules))
			{
				userRules = new Dictionary<Guid, AccessRuleValue>();
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

		return new Precomputed(blockAncestors, blocksByTag, userGroups, directUserGroupRules);
	}

	private static UserAccessEntity CalculateUserAccess(UserMemoryDto user, IInventoryCacheState state, Precomputed precomputed, HashSets hashSets)
	{
		Guid userGuid = user.Guid;

		// Глобальные правила пользователя
		var globalRule = hashSets.UserGlobalRules.TryGetValue(userGuid, out var userGlobalRule) ? userGlobalRule : AccessRuleValue.GetDefault();

		// Оптимизация: объединение групповых правил
		Dictionary<int, AccessRuleValue> groupSourceRules = null!;
		Dictionary<int, AccessRuleValue> groupBlockRules = null!;
		Dictionary<int, AccessRuleValue> groupTagRules = null!;
		Dictionary<Guid, AccessRuleValue> groupRules = [];

		if (precomputed.GroupsByUser.TryGetValue(userGuid, out var userGroupSet))
		{
			// Глобальные правила групп
			foreach (var groupGuid in userGroupSet)
			{
				if (hashSets.GroupGlobalRules.TryGetValue(groupGuid, out var groupRule) &&
						groupRule.Access > globalRule.Access)
				{
					globalRule = groupRule;
				}
			}

			// Получаем прямые правила пользователя
			precomputed.DirectUserGroupRules.TryGetValue(userGuid, out var userDirectGroupRules);

			foreach (var groupGuid in userGroupSet)
			{
				// Проверяем наличие прямого правила
				if (userDirectGroupRules != null &&
						userDirectGroupRules.TryGetValue(groupGuid, out var rule))
				{
					groupRules[groupGuid] = rule;
				}
			}

			// Предварительное объединение групповых правил
			groupSourceRules = new Dictionary<int, AccessRuleValue>();
			groupBlockRules = new Dictionary<int, AccessRuleValue>();
			groupTagRules = new Dictionary<int, AccessRuleValue>();

			foreach (var groupGuid in userGroupSet)
			{
				// Для источников
				if (hashSets.GroupRulesToSources.TryGetValue(groupGuid, out var rules))
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
				if (hashSets.GroupRulesToBlocks.TryGetValue(groupGuid, out rules))
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
				if (hashSets.GroupRulesToTags.TryGetValue(groupGuid, out rules))
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

		// Пропускаем расчет объектов для администраторов и заблокированных
		if (globalRule.Access is AccessType.Admin or AccessType.None)
		{
			return new UserAccessEntity(userGuid, user.EnergoIdGuid, globalRule, groupRules);
		}

		// Получаем пользовательские правила
		Dictionary<int, AccessRuleValue> userSourceRules = [];
		Dictionary<int, AccessRuleValue> userBlockRules = [];
		Dictionary<int, AccessRuleValue> userTagRules = [];
		hashSets.UserRulesToSources.TryGetValue(userGuid, out var userDirectSourceRules);
		hashSets.UserRulesToBlocks.TryGetValue(userGuid, out var userDirectBlockRules);
		hashSets.UserRulesToTags.TryGetValue(userGuid, out var userDirectTagRules);

		// 1. Расчет прав для источников
		foreach (var source in state.Sources)
		{
			int sourceId = source.Key;
			var rule = globalRule;

			// Пользовательские правила
			if (userDirectSourceRules != null &&
					userDirectSourceRules.TryGetValue(sourceId, out var userRule))
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
				userSourceRules[sourceId] = rule;
			}
		}

		// 2. Расчет прав для блоков
		foreach (var block in state.Blocks)
		{
			int blockId = block.Key;
			var rule = globalRule;

			// Пользовательские правила для блока
			if (userDirectBlockRules != null &&
					userDirectBlockRules.TryGetValue(blockId, out var userRule))
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
			if (precomputed.BlockAncestors.TryGetValue(blockId, out var ancestors))
			{
				foreach (var ancestorId in ancestors)
				{
					// Пользовательские правила предка
					if (userDirectBlockRules != null &&
							userDirectBlockRules.TryGetValue(ancestorId, out userRule) &&
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
				userBlockRules[blockId] = rule;
			}
		}

		// 3. Расчет прав для тегов
		foreach (var tag in state.Tags)
		{
			int tagId = tag.Key;
			var rule = globalRule;

			// Прямые пользовательские правила
			if (userDirectTagRules != null &&
					userDirectTagRules.TryGetValue(tagId, out var userRule))
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
			if (precomputed.BlocksByTag.TryGetValue(tagId, out var blockIds))
			{
				foreach (var blockId in blockIds)
				{
					if (userBlockRules.TryGetValue(blockId, out var blockRule) &&
							blockRule.Access > rule.Access)
					{
						rule = blockRule;
					}
				}
			}

			if (rule.Access > globalRule.Access)
			{
				userTagRules[tagId] = rule;
			}
		}

		// объект прав пользователя
		return new UserAccessEntity(userGuid, user.EnergoIdGuid, globalRule, groupRules, userSourceRules, userBlockRules, userTagRules);
	}

	// Вспомогательный метод для добавления правил
	private static void AddToMap<TKey, TValue>(
		Dictionary<TKey, Dictionary<TValue, AccessRuleValue>> map,
		TKey owner,
		TValue objId,
		AccessRuleValue rule)
		where TKey : notnull
		where TValue : notnull
	{
		if (!map.TryGetValue(owner, out var innerMap))
		{
			innerMap = new Dictionary<TValue, AccessRuleValue>();
			map[owner] = innerMap;
		}
		innerMap[objId] = rule;
	}

	private static ParallelOptions ParallelOptions { get; } = new ParallelOptions
	{
		MaxDegreeOfParallelism = Environment.ProcessorCount / 2
	};

	private record struct Precomputed(
		Dictionary<int, HashSet<int>> BlockAncestors,
		Dictionary<int, HashSet<int>> BlocksByTag,
		Dictionary<Guid, HashSet<Guid>> GroupsByUser,
		Dictionary<Guid, Dictionary<Guid, AccessRuleValue>> DirectUserGroupRules);

	private record struct HashSets(
		Dictionary<Guid, AccessRuleValue> UserGlobalRules,
		Dictionary<Guid, AccessRuleValue> GroupGlobalRules,
		Dictionary<Guid, Dictionary<int, AccessRuleValue>> UserRulesToSources,
		Dictionary<Guid, Dictionary<int, AccessRuleValue>> UserRulesToBlocks,
		Dictionary<Guid, Dictionary<int, AccessRuleValue>> UserRulesToTags,
		Dictionary<Guid, Dictionary<int, AccessRuleValue>> GroupRulesToSources,
		Dictionary<Guid, Dictionary<int, AccessRuleValue>> GroupRulesToBlocks,
		Dictionary<Guid, Dictionary<int, AccessRuleValue>> GroupRulesToTags);
}
