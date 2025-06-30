using Datalake.Database.InMemory.Models;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;

namespace Datalake.Database.Functions;

/// <summary>
/// 
/// </summary>
public static class AccessFunctions
{
	/// <summary>
	/// 
	/// </summary>
	public readonly static AccessRuleInfo DefaultRule = new(0, AccessType.NotSet);

	/// <summary>
	/// Вычисляет и возвращает карту доступа для каждого пользователя.
	/// </summary>
	public static Dictionary<Guid, UserAuthInfo> ComputeAccessDeepseeked(DatalakeDataState state)
	{
		// распределяем правила
		Dictionary<Guid, AccessRuleInfo> userGlobalRules = [];
		Dictionary<Guid, AccessRuleInfo> groupGlobalRules = [];
		Dictionary<Guid, Dictionary<int, AccessRuleInfo>> userRulesToSources = [];
		Dictionary<Guid, Dictionary<int, AccessRuleInfo>> groupRulesToSources = [];
		Dictionary<Guid, Dictionary<int, AccessRuleInfo>> userRulesToBlocks = [];
		Dictionary<Guid, Dictionary<int, AccessRuleInfo>> groupRulesToBlocks = [];
		Dictionary<Guid, Dictionary<int, AccessRuleInfo>> userRulesToTags = [];
		Dictionary<Guid, Dictionary<int, AccessRuleInfo>> groupRulesToTags = [];
		foreach (var r in state.AccessRights)
		{
			var rule = new AccessRuleInfo(RuleId: r.Id, Access: r.AccessType);
			Guid guid;

			if (r.UserGuid.HasValue)
			{
				guid = r.UserGuid.Value;

				if (r.IsGlobal)
				{
					userGlobalRules.Add(guid, rule);
				}
				else if (r.SourceId.HasValue)
				{
					if (!userRulesToSources.TryGetValue(guid, out var rules))
					{
						rules = [];
						userRulesToSources[guid] = rules;
					}
					rules[r.SourceId.Value] = rule;
				}
				else if (r.BlockId.HasValue)
				{
					if (!userRulesToBlocks.TryGetValue(guid, out var rules))
					{
						rules = [];
						userRulesToBlocks[guid] = rules;
					}
					rules[r.BlockId.Value] = rule;
				}
				else if (r.TagId.HasValue)
				{
					if (!userRulesToTags.TryGetValue(guid, out var rules))
					{
						rules = [];
						userRulesToTags[guid] = rules;
					}
					rules[r.TagId.Value] = rule;
				}
			}
			else if (r.UserGroupGuid.HasValue)
			{
				guid = r.UserGroupGuid.Value;

				if (r.IsGlobal)
				{
					groupGlobalRules.Add(guid, rule);
				}
				else if (r.SourceId.HasValue)
				{
					if (!groupRulesToSources.TryGetValue(guid, out var rules))
					{
						rules = [];
						groupRulesToSources[guid] = rules;
					}
					rules[r.SourceId.Value] = rule;
				}
				else if (r.BlockId.HasValue)
				{
					if (!groupRulesToBlocks.TryGetValue(guid, out var rules))
					{
						rules = [];
						groupRulesToBlocks[guid] = rules;
					}
					rules[r.BlockId.Value] = rule;
				}
				else if (r.TagId.HasValue)
				{
					if (!groupRulesToTags.TryGetValue(guid, out var rules))
					{
						rules = [];
						groupRulesToTags[guid] = rules;
					}
					rules[r.TagId.Value] = rule;
				}
			}
		}

		// РАСЧЕТ
		// Доступ это уровни в порядке приоритета: не задано < читатель < редактор < менеджер < запрещено < администратор.
		// При проверках выбирается максимальное значение доступа из всех действующих правил и сравнивается с минимально требуемым. При этом мы должны запомнить, какое правило в итоге определило доступ.
		// Если ни одного правила нет, применяется правило по умолчанию с наименьшим уровнем доступа из существующих.
		// Пользователь имеет глобальное правило доступа. Оно проверяется при доступе к любому объекту системы, при его достаточности остальные правила уже не нужны
		// Группа имеет глобальное правило доступа. Оно проверяется для доступа к любому объекту, но только для пользователей, которые состоят в этой группе или в нижестоящих по иерархии.
		// Есть иерархия объектов. Блоки имеют древовидную структуру. Каждый блок может содержать теги. Теги могут быть в нескольких блоках одновременно.
		// Кроме этого, есть прямые правила на конкретный объект. Прямые правила могут быть выданы как пользователю, так и группе. При этом прямое правило для группы действует и на нижестоящие группы.
		// При действии прямого правила на объект с другими подчиненными объектами это прямое правило распространяется и на эти подчиненные объекты.

		// Таким образом, доступ пользователя к конкретному объекту будет выбираться из коллекции:
		// правило по умолчанию с доступом "не задано"
		// глобальное правило пользователя
		// глобальные правила групп, к которым он в итоге относится
		// прямое правило пользователя на объект
		// прямые правила пользователя на объекты выше в иерархии объектов

		// необходимо составить эти коллекции, вычислить самое сильное по доступу правило и сохранить эту информацию в виде словаря по каждому типу объектов: идентификатор объекта = идентификатор и значение доступа выбранного правила (или ссылка на само правило)

		// Подготовка данных для иерархий
		var blocksById = state.Blocks.ToDictionary(b => b.Id, b => b);
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

		var userDirectGroups = new Dictionary<Guid, HashSet<Guid>>();
		foreach (var relation in state.UserGroupRelations)
		{
			if (!userDirectGroups.TryGetValue(relation.UserGuid, out var groups))
			{
				groups = new HashSet<Guid>();
				userDirectGroups[relation.UserGuid] = groups;
			}
			groups.Add(relation.UserGroupGuid);
		}

		var userGroups = new Dictionary<Guid, HashSet<Guid>>();
		foreach (var user in state.Users)
		{
			var groupsSet = new HashSet<Guid>();
			if (userDirectGroups.TryGetValue(user.Guid, out var directGroups))
			{
				foreach (var groupGuid in directGroups)
				{
					if (groupAncestors.TryGetValue(groupGuid, out var ancestors))
					{
						groupsSet.UnionWith(ancestors);
					}
				}
			}
			userGroups[user.Guid] = groupsSet;
		}

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

		// Расчет прав для каждого пользователя
		Dictionary<Guid, UserAuthInfo> usersAccess = [];
		foreach (var user in state.Users)
		{
			Guid userGuid = user.Guid;
			UserAuthInfo access = new()
			{
				Guid = userGuid,
				FullName = user.FullName ?? user.Login ?? string.Empty,
				RootRule = DefaultRule,
				Token = string.Empty,
			};
			AccessRuleInfo globalRule = DefaultRule;

			// Глобальное правило пользователя
			if (userGlobalRules.TryGetValue(userGuid, out var userGlobalRule))
				globalRule = userGlobalRule;

			// Глобальные правила групп
			if (userGroups.TryGetValue(userGuid, out var userGroupSet))
			{
				foreach (var groupGuid in userGroupSet)
				{
					if (groupGlobalRules.TryGetValue(groupGuid, out var groupRule) &&
							groupRule.Access > globalRule.Access)
					{
						globalRule = groupRule;
					}
				}
			}
			access.RootRule = globalRule;

			if (globalRule.Access != AccessType.Admin && globalRule.Access != AccessType.NoAccess)
			{
				// Доступ к источникам
				foreach (var source in state.Sources)
				{
					int sourceId = source.Id;
					AccessRuleInfo rule = globalRule;

					if (userRulesToSources.TryGetValue(userGuid, out var userSourceRules) &&
							userSourceRules.TryGetValue(sourceId, out var userRule))
					{
						rule = userRule;
					}

					if (userGroupSet != null)
					{
						foreach (var groupGuid in userGroupSet)
						{
							if (groupRulesToSources.TryGetValue(groupGuid, out var groupRules) &&
									groupRules.TryGetValue(sourceId, out var groupRule) &&
									groupRule.Access > rule.Access)
							{
								rule = groupRule;
							}
						}
					}

					if (rule.Access > globalRule.Access)
						access.Sources[sourceId] = rule;
				}

				// Доступ к блокам
				foreach (var block in state.Blocks)
				{
					int blockId = block.Id;
					AccessRuleInfo rule = globalRule;

					if (userRulesToBlocks.TryGetValue(userGuid, out var userBlockRules) &&
							userBlockRules.TryGetValue(blockId, out var userRule))
					{
						rule = userRule;
					}

					if (userGroupSet != null)
					{
						foreach (var groupGuid in userGroupSet)
						{
							if (groupRulesToBlocks.TryGetValue(groupGuid, out var groupRules) &&
									groupRules.TryGetValue(blockId, out var groupRule) &&
									groupRule.Access > rule.Access)
							{
								rule = groupRule;
							}
						}
					}

					if (blockAncestors.TryGetValue(blockId, out var ancestors))
					{
						foreach (var ancestorId in ancestors)
						{
							if (userBlockRules != null &&
									userBlockRules.TryGetValue(ancestorId, out var userAncestorRule) &&
									userAncestorRule.Access > rule.Access)
							{
								rule = userAncestorRule;
							}

							if (userGroupSet != null)
							{
								foreach (var groupGuid in userGroupSet)
								{
									if (groupRulesToBlocks.TryGetValue(groupGuid, out var groupRules) &&
											groupRules.TryGetValue(ancestorId, out var groupAncestorRule) &&
											groupAncestorRule.Access > rule.Access)
									{
										rule = groupAncestorRule;
									}
								}
							}
						}
					}

					if (rule.Access > globalRule.Access)
						access.Blocks[blockId] = rule;
				}

				// Доступ к тегам
				foreach (var tag in state.Tags)
				{
					int tagId = tag.Id;
					AccessRuleInfo rule = globalRule;

					if (userRulesToTags.TryGetValue(userGuid, out var userTagRules) &&
							userTagRules.TryGetValue(tagId, out var userRule))
					{
						rule = userRule;
					}

					if (userGroupSet != null)
					{
						foreach (var groupGuid in userGroupSet)
						{
							if (groupRulesToTags.TryGetValue(groupGuid, out var groupRules) &&
									groupRules.TryGetValue(tagId, out var groupRule) &&
									groupRule.Access > rule.Access)
							{
								rule = groupRule;
							}
						}
					}

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
						access.Tags[tagId] = rule;
				}
			}

			usersAccess[userGuid] = access;
		}

		return usersAccess;
	}

	/// <summary>
	/// Вычисляет и возвращает карту доступа для каждого пользователя.
	/// </summary>
	public static Dictionary<Guid, UserAuthInfo> ComputeAccessCopiloted(DatalakeDataState state)
	{
		// 1) Собираем все пользовательские и групповые правила по глобальным, источникам, блокам и тегам.
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
					userGlobalRules[userId] = rule;
				else if (r.SourceId.HasValue)
					AddToMap(userRulesToSources, userId, r.SourceId.Value, rule);
				else if (r.BlockId.HasValue)
					AddToMap(userRulesToBlocks, userId, r.BlockId.Value, rule);
				else if (r.TagId.HasValue)
					AddToMap(userRulesToTags, userId, r.TagId.Value, rule);
			}
			else if (r.UserGroupGuid.HasValue)
			{
				var grpId = r.UserGroupGuid.Value;
				if (r.IsGlobal)
					groupGlobalRules[grpId] = rule;
				else if (r.SourceId.HasValue)
					AddToMap(groupRulesToSources, grpId, r.SourceId.Value, rule);
				else if (r.BlockId.HasValue)
					AddToMap(groupRulesToBlocks, grpId, r.BlockId.Value, rule);
				else if (r.TagId.HasValue)
					AddToMap(groupRulesToTags, grpId, r.TagId.Value, rule);
			}
		}

		// 2) Индексируем связи "блок → дети" и "группа → дети" для быстрого обхода и вычисления замыкания.
		var blockChildren = new Dictionary<int, List<int>>();
		foreach (var b in state.Blocks)
		{
			if (b.ParentId.HasValue)
			{
				if (!blockChildren.TryGetValue(b.ParentId.Value, out var list))
					blockChildren[b.ParentId.Value] = list = new();
				list.Add(b.Id);
			}
		}

		var groupChildren = new Dictionary<Guid, List<Guid>>();
		foreach (var g in state.UserGroups)
		{
			if (g.ParentGuid.HasValue)
			{
				if (!groupChildren.TryGetValue(g.ParentGuid.Value, out var list))
					groupChildren[g.ParentGuid.Value] = list = new();
				list.Add(g.Guid);
			}
		}

		// 3) Индексируем прямые "группа → пользователи"
		var groupDirectUsers = new Dictionary<Guid, List<Guid>>();
		foreach (var rel in state.UserGroupRelations)
		{
			if (!groupDirectUsers.TryGetValue(rel.UserGroupGuid, out var list))
				groupDirectUsers[rel.UserGroupGuid] = list = new();
			list.Add(rel.UserGuid);
		}

		// 4) Вычисляем полные списки пользователей в группах (учитывая иерархию)
		var groupUsersMap = new Dictionary<Guid, HashSet<Guid>>();
		foreach (var g in state.UserGroups)
		{
			var descendants = CollectDescendants(g.Guid, groupChildren);
			var users = new HashSet<Guid>();
			foreach (var dg in descendants)
			{
				if (groupDirectUsers.TryGetValue(dg, out var direct))
					foreach (var u in direct)
						users.Add(u);
			}
			groupUsersMap[g.Guid] = users;
		}

		// 5) Делаем обратный индекс: для каждого пользователя список групп, куда он входит
		var userToGroups = new Dictionary<Guid, List<Guid>>();
		foreach (var kv in groupUsersMap)
		{
			var grp = kv.Key;
			foreach (var u in kv.Value)
			{
				if (!userToGroups.TryGetValue(u, out var list))
					userToGroups[u] = list = new();
				list.Add(grp);
			}
		}

		// 6) Собираем список всех пользователей для расчета
		var allUsers = new HashSet<Guid>(
				userGlobalRules.Keys
				.Union(userRulesToSources.Keys)
				.Union(userRulesToBlocks.Keys)
				.Union(userRulesToTags.Keys)
				.Union(userToGroups.Keys)
		);

		// 7) Предварительно индексируем связи тегов и блоков
		var blockTagsDirect = new Dictionary<int, List<int>>();
		foreach (var bt in state.BlockTags)
		{
			if (!bt.TagId.HasValue)
				continue;
			if (!blockTagsDirect.TryGetValue(bt.BlockId, out var list))
				blockTagsDirect[bt.BlockId] = list = new();
			list.Add(bt.TagId.Value);
		}

		// Для каждого блока вычислим полное множество его потомков (включая себя),
		// а также все теги, которые встречаются в любом потомке.
		var blockDescendants = new Dictionary<int, HashSet<int>>();
		var blockAllTags = new Dictionary<int, HashSet<int>>();

		foreach (var b in state.Blocks)
		{
			// собрать всех потомков через DFS/BFS
			var desc = CollectDescendants(b.Id, blockChildren);
			blockDescendants[b.Id] = desc;

			// собрать теги из всех потомков
			var tags = new HashSet<int>();
			foreach (var d in desc)
			{
				if (blockTagsDirect.TryGetValue(d, out var tlist))
					foreach (var t in tlist)
						tags.Add(t);
			}
			blockAllTags[b.Id] = tags;
		}

		// 8) Собираем список всех тегов системы (из связей и правил)
		var allTags = new HashSet<int>(
				state.BlockTags.Where(x => x.TagId.HasValue).Select(x => x.TagId.Value)
				.Union(state.AccessRights.Where(x => x.TagId.HasValue).Select(x => x.TagId.Value))
		);

		// 9) Основной проход: для каждого пользователя — рассчитываем доступ
		var result = new Dictionary<Guid, UserAuthInfo>(allUsers.Count);
		foreach (var userInfo in state.Users)
		{
			var user = userInfo.Guid;
			var ua = new UserAuthInfo
			{
				FullName = userInfo.FullName ?? userInfo.Login ?? string.Empty,
				Guid = user,
				Token = string.Empty,
				RootRule = DefaultRule,
			};
			result[user] = ua;

			// 9.1) ROOT-правило: выбираем наибольшее из default, userGlobal, groupGlobal
			ua.RootRule = Max(ua.RootRule,
												userGlobalRules.TryGetValue(user, out var ugr) ? ugr : DefaultRule);
			if (userToGroups.TryGetValue(user, out var myGroups))
			{
				foreach (var g in myGroups)
				{
					if (groupGlobalRules.TryGetValue(g, out var ggr))
						ua.RootRule = Max(ua.RootRule, ggr);
				}
			}

			// 9.2) ПО ИСТОЧНИКАМ
			foreach (var src in state.Sources)
			{
				var best = ua.RootRule; // глобальный уже проверили, но для ясности включаем
				if (userRulesToSources.TryGetValue(user, out var urSrc) &&
						urSrc.TryGetValue(src.Id, out var ur))
					best = Max(best, ur);

				if (myGroups != null)
				{
					foreach (var g in myGroups)
					{
						if (groupRulesToSources.TryGetValue(g, out var grSrc) &&
								grSrc.TryGetValue(src.Id, out var gr))
							best = Max(best, gr);
					}
				}

				ua.Sources[src.Id] = best;
			}

			// 9.3) ПО БЛОКАМ (учитываем иерархию родительских блоков)
			// для быстрого поиска предков: карта блок→parent
			var parentMap = state.Blocks.ToDictionary(b => b.Id, b => b.ParentId);

			foreach (var block in state.Blocks)
			{
				var best = ua.RootRule;
				// собственные прямые
				if (userRulesToBlocks.TryGetValue(user, out var urBlk) &&
						urBlk.TryGetValue(block.Id, out var ubr))
					best = Max(best, ubr);

				if (myGroups != null)
				{
					foreach (var g in myGroups)
					{
						if (groupRulesToBlocks.TryGetValue(g, out var grBlk) &&
								grBlk.TryGetValue(block.Id, out var gbr))
							best = Max(best, gbr);
					}
				}

				// наследуем правила по цепочке родителей
				var cur = parentMap[block.Id];
				while (cur.HasValue)
				{
					if (userRulesToBlocks.TryGetValue(user, out urBlk) &&
							urBlk.TryGetValue(cur.Value, out var up))
						best = Max(best, up);

					if (myGroups != null)
					{
						foreach (var g in myGroups)
						{
							if (groupRulesToBlocks.TryGetValue(g, out var grBlk2) &&
									grBlk2.TryGetValue(cur.Value, out var gp))
								best = Max(best, gp);
						}
					}

					cur = parentMap[cur.Value];
				}

				ua.Blocks[block.Id] = best;
			}

			// 9.4) ПО ТЕГАМ (учитываем прямые правила + правила по блокам, в которых тег находится)
			// строим обратную карту тег→список блоков
			var tagToBlocks = new Dictionary<int, List<int>>();
			foreach (var kv in blockAllTags)
				foreach (var t in kv.Value)
				{
					if (!tagToBlocks.TryGetValue(t, out var blist))
						tagToBlocks[t] = blist = new();
					blist.Add(kv.Key);
				}

			foreach (var tagId in allTags)
			{
				var best = ua.RootRule;
				// прямые по тегу
				if (userRulesToTags.TryGetValue(user, out var urTag) &&
						urTag.TryGetValue(tagId, out var ut))
					best = Max(best, ut);

				if (myGroups != null)
				{
					foreach (var g in myGroups)
					{
						if (groupRulesToTags.TryGetValue(g, out var grTag) &&
								grTag.TryGetValue(tagId, out var gt))
							best = Max(best, gt);
					}
				}

				// правила по связанным блокам: ищем все блоки, где встречается этот тег
				if (tagToBlocks.TryGetValue(tagId, out var blocksContaining))
				{
					foreach (var blkId in blocksContaining)
					{
						// прямые на блок
						if (userRulesToBlocks.TryGetValue(user, out var urb2) &&
								urb2.TryGetValue(blkId, out var urbd2))
							best = Max(best, urbd2);

						if (myGroups != null)
						{
							foreach (var g in myGroups)
							{
								if (groupRulesToBlocks.TryGetValue(g, out var grb2) &&
										grb2.TryGetValue(blkId, out var grbval))
									best = Max(best, grbval);
							}
						}

						// + наследуем по предкам блока
						var cur2 = parentMap[blkId];
						while (cur2.HasValue)
						{
							if (userRulesToBlocks.TryGetValue(user, out var urb3) &&
									urb3.TryGetValue(cur2.Value, out var urbd3))
								best = Max(best, urbd3);

							if (myGroups != null)
							{
								foreach (var g in myGroups)
								{
									if (groupRulesToBlocks.TryGetValue(g, out var grb3) &&
											grb3.TryGetValue(cur2.Value, out var grb3v))
										best = Max(best, grb3v);
								}
							}

							cur2 = parentMap[cur2.Value];
						}
					}
				}

				ua.Tags[tagId] = best;
			}
		}

		return result;
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="access"></param>
	/// <returns></returns>
	public static DatalakeAccessState CreateAccessState(Dictionary<Guid, UserAuthInfo> access) => new(access);

	#region Helpers

	/// <summary>
	/// Добавляет правило в вложенную карту userOrGroup → (objId → rule).
	/// </summary>
	private static void AddToMap<TKey, TObject>(
			Dictionary<TKey, Dictionary<TObject, AccessRuleInfo>> map,
			TKey owner,
			TObject objId,
			AccessRuleInfo rule)
	{
		if (!map.TryGetValue(owner, out var inner))
			map[owner] = inner = new();
		inner[objId] = rule;
	}

	/// <summary>
	/// Возвращает максимум по значению AccessType (сохраняя информацию о RuleId).
	/// </summary>
	private static AccessRuleInfo Max(AccessRuleInfo a, AccessRuleInfo b)
			=> a.Access >= b.Access ? a : b;

	/// <summary>
	/// Собирает в HashSet все descendant-узлы, включая корень, по заданной карте parent→children.
	/// </summary>
	private static HashSet<T> CollectDescendants<T>(
			T root,
			Dictionary<T, List<T>> childrenMap)
	{
		var result = new HashSet<T> { root };
		var stack = new Stack<T>();
		stack.Push(root);

		while (stack.Count > 0)
		{
			var cur = stack.Pop();
			if (childrenMap.TryGetValue(cur, out var children))
			{
				foreach (var c in children)
				{
					if (result.Add(c))
						stack.Push(c);
				}
			}
		}

		return result;
	}

	#endregion
}
