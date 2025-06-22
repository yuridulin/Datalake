using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.Database.InMemory;

/// <summary>
/// Хранилище производных данных
/// </summary>
public class DatalakeDerivedDataStore
{
	/// <summary>Конструктор</summary>
	public DatalakeDerivedDataStore(DatalakeDataStore stateHolder)
	{
		stateHolder.StateChanged += (_, newState) =>
		{
			if (newState.Version > _lastProcessingVersion)
			{
				_lastProcessingVersion = newState.Version;

				Task.Run(() =>
				{
					RebuildTree(newState);
					RebuildUserRightsCacheOptimized(newState);
				});
			}
		};
	}

	private long _lastProcessingVersion = -1;

	#region Дерево блоков

	private BlockTreeInfo[] _cachedBlockTree = [];

	private void RebuildTree(DatalakeDataState state)
	{
		var tagsDict = state.Tags.ToDictionary(x => x.Id);

		var blocksWithTags = state.Blocks
			.Select(block => new BlockWithTagsInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				ParentId = block.ParentId,
				Description = block.Description,
				Name = block.Name,
				Tags = state.BlockTags
					.Where(x => x.BlockId == block.Id)
					.Select(x =>
					{
						if (x.TagId.HasValue && tagsDict.TryGetValue(x.TagId.Value, out var tag))
						{
							return new BlockNestedTagInfo
							{
								Id = tag.Id,
								Guid = tag.GlobalGuid,
								Name = tag.Name,
								Frequency = tag.Frequency,
								Type = tag.Type,
								SourceType = SourceType.NotSet,
								LocalName = x.Name ?? string.Empty,
								Relation = x.Relation,
								SourceId = tag.SourceId,
							};
						}
						else
							return null!;
					})
					.Where(x => x != null)
					.ToArray(),
			})
			.ToArray();

		var nextBlockTree = ReadBlockChildren(blocksWithTags, null, string.Empty);

		Interlocked.Exchange(ref _cachedBlockTree, nextBlockTree);
	}

	private static BlockTreeInfo[] ReadBlockChildren(BlockWithTagsInfo[] blocks, int? id, string prefix)
	{
		var prefixString = prefix + (string.IsNullOrEmpty(prefix) ? string.Empty : ".");
		return blocks
			.Where(x => x.ParentId == id)
			.Select(x =>
			{
				var block = new BlockTreeInfo
				{
					Id = x.Id,
					Guid = x.Guid,
					ParentId = x.ParentId,
					Name = x.Name,
					FullName = prefixString + x.Name,
					Description = x.Description,
					Tags = x.Tags
						.Select(tag => new BlockNestedTagInfo
						{
							Guid = tag.Guid,
							Name = tag.Name,
							Id = tag.Id,
							Relation = tag.Relation,
							SourceId = tag.SourceId,
							LocalName = tag.LocalName,
							Type = tag.Type,
							Frequency = tag.Frequency,
							SourceType = tag.SourceType,
						})
						.ToArray(),
					AccessRule = x.AccessRule,
					Children = ReadBlockChildren(blocks, x.Id, prefixString + x.Name),
				};

				/*if (!x.AccessRule.AccessType.HasAccess(AccessType.Viewer))
				{
					block.Name = string.Empty;
					block.Description = string.Empty;
					block.Tags = [];
				}*/

				return block;
			})
			/*.Where(x => x.Children.Length > 0 || x.AccessRule.AccessType.HasAccess(AccessType.Viewer))*/
			.OrderBy(x => x.Name)
			.ToArray();
	}

	/// <summary>
	/// Получение дерева блоков со списком полей каждого блока
	/// </summary>
	/// <returns>Коллекция корневых элементом дерева</returns>
	public BlockTreeInfo[] BlocksTree() => _cachedBlockTree;

	#endregion

	#region Права пользователей

	private Dictionary<Guid, UserAuthInfo> _rights = [];

	private void RebuildUserRightsCacheOptimized(DatalakeDataState state)
	{
		AccessRights defaultRule = new() { Id = 0, AccessType = AccessType.NotSet };

		#region ПОДГОТОВКА

		// Словари для быстрого доступа
		var userGroupsDict = state.UserGroups.ToDictionary(x => x.Guid);
		var blocksDict = state.Blocks.ToDictionary(x => x.Id);
		var sourcesDict = state.Sources.ToDictionary(x => x.Id);
		var tagsDict = state.Tags.ToDictionary(x => x.Id);

		// Предварительный расчет иерархии групп
		var groupAncestors = new Dictionary<Guid, List<Guid>>();
		foreach (var group in state.UserGroups)
		{
			var ancestors = new List<Guid>();
			Guid? current = group.Guid;
			while (current.HasValue && userGroupsDict.ContainsKey(current.Value))
			{
				ancestors.Add(current.Value);
				current = userGroupsDict[current.Value].ParentGuid;
			}
			groupAncestors[group.Guid] = ancestors;
		}

		// Предварительный расчет иерархии блоков
		var blockAncestors = new Dictionary<int, int[]>();
		foreach (var block in state.Blocks)
		{
			var ancestors = new List<int>();
			int? current = block.Id;
			while (current.HasValue && blocksDict.ContainsKey(current.Value))
			{
				ancestors.Add(current.Value);
				current = blocksDict[current.Value].ParentId;
			}
			blockAncestors[block.Id] = ancestors.ToArray();
		}

		// Индексы для прав групп
		var groupGlobalRights = state.AccessRights
			.Where(r => r.IsGlobal && r.UserGroupGuid.HasValue)
			.ToDictionary(r => r.UserGroupGuid!.Value);

		var groupBlockRights = state.AccessRights
			.Where(r => r.BlockId.HasValue && r.UserGroupGuid.HasValue)
			.ToLookup(r => r.UserGroupGuid!.Value)
			.ToDictionary(g => g.Key, g => g.ToDictionary(r => r.BlockId!.Value));

		var groupSourceRights = state.AccessRights
			.Where(r => r.SourceId.HasValue && r.UserGroupGuid.HasValue)
			.ToLookup(r => r.UserGroupGuid!.Value)
			.ToDictionary(g => g.Key, g => g.ToDictionary(r => r.SourceId!.Value));

		var groupTagRights = state.AccessRights
			.Where(r => r.TagId.HasValue && r.UserGroupGuid.HasValue)
			.ToLookup(r => r.UserGroupGuid!.Value)
			.ToDictionary(g => g.Key, g => g.ToDictionary(r => r.TagId!.Value));

		// Индексы для прав пользователей
		var userGlobalRights = state.AccessRights
			.Where(r => r.IsGlobal && r.UserGuid.HasValue)
			.ToDictionary(r => r.UserGuid!.Value);

		var userBlockRights = state.AccessRights
			.Where(r => r.BlockId.HasValue && r.UserGuid.HasValue)
			.ToLookup(r => r.UserGuid!.Value)
			.ToDictionary(g => g.Key, g => g.ToDictionary(r => r.BlockId!.Value));

		var userSourceRights = state.AccessRights
			.Where(r => r.SourceId.HasValue && r.UserGuid.HasValue)
			.ToLookup(r => r.UserGuid!.Value)
			.ToDictionary(g => g.Key, g => g.ToDictionary(r => r.SourceId!.Value));

		var userTagRights = state.AccessRights
			.Where(r => r.TagId.HasValue && r.UserGuid.HasValue)
			.ToLookup(r => r.UserGuid!.Value)
			.ToDictionary(g => g.Key, g => g.ToDictionary(r => r.TagId!.Value));

		// Связи тегов с блоками
		var tagBlocksRelation = state.BlockTags
			.GroupBy(tr => tr.TagId!.Value)
			.ToDictionary(g => g.Key, g => g.Select(tr => tr.BlockId).ToArray());

		// Связи пользователей с группами
		var userGroupsRelation = state.UserGroupRelations
			.GroupBy(ur => ur.UserGuid)
			.ToDictionary(g => g.Key, g => g.ToDictionary(ur => ur.UserGroupGuid));

		#endregion

		#region РАСЧЕТ ГРУПП

		var userGroupsRights = state.UserGroups
			.Select(group =>
			{
				groupBlockRights.TryGetValue(group.Guid, out var directAccessToBlock);
				groupSourceRights.TryGetValue(group.Guid, out var directAccessToSource);
				groupTagRights.TryGetValue(group.Guid, out var directAccessToTag);
				var globalRule = groupGlobalRights.TryGetValue(group.Guid, out var rr) ? rr : defaultRule;

				var groupBlocks = state.Blocks
					.Select(block =>
					{
						var chosenRule = globalRule;
						var ancestors = blockAncestors[block.Id];

						if (directAccessToBlock != null)
						{
							foreach (var id in ancestors)
							{
								if (directAccessToBlock.TryGetValue(id, out var candidateRule))
								{
									if (candidateRule.AccessType > chosenRule.AccessType)
									{
										chosenRule = candidateRule;
									}
								}
							}
						}

						return new
						{
							block.Id,
							block.Name,
							IdWithParents = ancestors,
							Rule = chosenRule,
						};
					})
					.ToArray();

				var groupSources = state.Sources
					.Select(source =>
					{
						var chosenRule = globalRule;

						if (directAccessToSource != null)
						{
							if (directAccessToSource.TryGetValue(source.Id, out var directRule))
							{
								if (directRule.AccessType > chosenRule.AccessType)
									chosenRule = directRule;
							}
						}

						return new
						{
							source.Id,
							source.Name,
							Rule = chosenRule,
						};
					})
					.ToArray();

				var groupTags = state.Tags
					.Select(tag =>
					{
						var chosenRule = globalRule;

						var blocks = tagBlocksRelation.TryGetValue(tag.Id, out var t) ? t : null;

						if (directAccessToTag != null)
						{
							if (directAccessToTag.TryGetValue(tag.Id, out var directRule))
							{
								if (directRule.AccessType > globalRule.AccessType)
									chosenRule = directRule;
							}
						}

						if (blocks != null)
						{
							var rulesFromBlocks = groupBlocks
								.Where(b => blocks?.Contains(b.Id) ?? false)
								.Select(x => x.Rule)
								.ToArray();

							foreach (var block in groupBlocks)
							{
								if (blocks.Contains(block.Id))
								{
									if (block.Rule.AccessType > chosenRule.AccessType)
										chosenRule = block.Rule;
								}
							}
						}

						return new
						{
							tag.Id,
							tag.Name,
							IdWithParents = blocks,
							Rule = chosenRule,
						};
					})
					.ToArray();

				return new
				{
					group.Guid,
					IdWithParents = groupAncestors[group.Guid],
					Sources = groupSources.ToDictionary(x => x.Id, x => x.Rule),
					Blocks = groupBlocks.ToDictionary(x => x.Id, x => x.Rule),
					Tags = groupTags.ToDictionary(x => x.Id, x => x.Rule),
				};
			})
			.ToArray();

		#endregion

		#region РАСЧЕТ ПОЛЬЗОВАТЕЛЕЙ

		var userRights = state.Users
			.Select(user =>
			{
				userGroupsRelation.TryGetValue(user.Guid, out var relationsToGroups);
				userBlockRights.TryGetValue(user.Guid, out var directAccessToBlock);
				userSourceRights.TryGetValue(user.Guid, out var directAccessToSource);
				userTagRights.TryGetValue(user.Guid, out var directAccessToTags);

				var globalRule = state.AccessRights.FirstOrDefault(x => x.UserGuid == user.Guid && x.IsGlobal)
					?? defaultRule;

				var userGroups = userGroupsRights
					.Select(x =>
					{
						var ruleSet = new List<UserGroupRelation>()
						{
						new() {
							AccessType = globalRule.AccessType,
							UserGroupGuid = x.Guid,
							UserGuid = user.Guid
						},
						};

						if (relationsToGroups != null)
						{
							var directRule = x.IdWithParents
								.Select(id => relationsToGroups.TryGetValue(id, out var r) ? r : null)
								.FirstOrDefault(r => r != null);

							if (directRule != null)
								ruleSet.Add(directRule);
						}

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							x.Guid,
							Rule = chosenRule,
							x.Sources,
							x.Blocks,
							x.Tags,
						};
					})
					.ToArray();

				var userBlocks = state.Blocks
					.Select(block =>
					{
						var ruleSet = new List<AccessRights>() { globalRule };

						if (directAccessToBlock != null)
						{
							var directRule = blockAncestors[block.Id]
								.Select(x => directAccessToBlock.TryGetValue(x, out var r) ? r : null)
								.FirstOrDefault(r => r != null);

							if (directRule != null)
								ruleSet.Add(directRule);
						}

						// так как мы уже вычислили права доступа каждой группы на каждый блок,
						// нам не нужно подниматься по иерархии еще раз
						var rulesFromGroups = userGroups
							.Select(groupMap => groupMap.Blocks.TryGetValue(block.Id, out var rule) ? rule : null)
							.Where(x => x != null)
							.ToArray();

						ruleSet.AddRange(rulesFromGroups!);

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							block.Id,
							Rule = chosenRule,
						};
					})
					.ToArray();

				var userSources = state.Sources
					.Select(source =>
					{
						var ruleSet = new List<AccessRights>() { globalRule };

						if (directAccessToSource != null)
						{
							if (directAccessToSource.TryGetValue(source.Id, out var directRule))
								ruleSet.Add(directRule);
						}

						var rulesFromGroups = userGroups
							.Select(groupMap => groupMap.Sources.TryGetValue(source.Id, out var rule) ? rule : null)
							.Where(x => x != null)
							.ToArray();

						ruleSet.AddRange(rulesFromGroups!);

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							source.Id,
							Rule = chosenRule,
						};
					})
					.ToArray();

				var userTags = state.Tags
					.Select(tag =>
					{
						var ruleSet = new List<AccessRights>() { globalRule };

						if (directAccessToTags != null)
						{
							if (directAccessToTags.TryGetValue(tag.Id, out var directRule))
								ruleSet.Add(directRule);
						}

						var blocks = tagBlocksRelation.TryGetValue(tag.Id, out var b) ? b : null;

						var rulesFromBlocks = userBlocks
							.Where(x => blocks?.Contains(x.Id) ?? false)
							.Select(x => x.Rule)
							.ToArray();
						ruleSet.AddRange(rulesFromBlocks);

						var rulesFromGroups = userGroups
							.Select(groupMap => groupMap.Tags.TryGetValue(tag.Id, out var rule) ? rule : null)
							.Where(x => x != null)
							.ToArray();
						ruleSet.AddRange(rulesFromGroups!);

						var chosenRule = ruleSet.OrderByDescending(x => x.AccessType).First();

						return new
						{
							tag.Id,
							tag.Name,
							tag.GlobalGuid,
							Rule = chosenRule,
						};
					})
					.ToArray();

				return new UserAuthInfo
				{
					Guid = user.Guid,
					FullName = user.FullName ?? string.Empty,
					Token = string.Empty,
					EnergoId = user.EnergoIdGuid,
					GlobalAccessType = globalRule.AccessType,
					Groups = userGroups
						.ToDictionary(x => x.Guid, x => new AccessRuleInfo { RuleId = x.Rule.Id, AccessType = x.Rule.AccessType, }),
					Sources = userSources
						.ToDictionary(x => x.Id, x => new AccessRuleInfo { RuleId = x.Rule.Id, AccessType = x.Rule.AccessType, }),
					Blocks = userBlocks
						.ToDictionary(x => x.Id, x => new AccessRuleInfo { RuleId = x.Rule.Id, AccessType = x.Rule.AccessType, }),
					Tags = userTags
						.ToDictionary(x => x.GlobalGuid, x => new AccessRuleInfo { RuleId = x.Rule.Id, AccessType = x.Rule.AccessType, }),
				};
			})
			.ToDictionary(x => x.Guid);

		#endregion

		Interlocked.Exchange(ref _rights, userRights);
	}

	/// <summary>
	/// Разрешения пользователей, рассчитанные на каждый объект системы
	/// </summary>
	/// <returns>Разрешения, сгруппированные по идентификатору пользователя</returns>
	public Dictionary<Guid, UserAuthInfo> CalculatedRights() => _rights;

	#endregion
}
