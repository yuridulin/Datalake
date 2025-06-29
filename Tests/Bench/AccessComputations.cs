using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Datalake.Database.InMemory.Models;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Bench;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class AccessComputations
{
	private DatalakeDataState _state = null!;

	[GlobalSetup]
	public void Setup()
	{
		_state = Generator.CreateTestData();
	}

	[Benchmark]
	public void Original()
	{
		Methods.Original(_state);
	}


	[Benchmark]
	public void Optimized()
	{
		Methods.Optimized(_state);
	}
}

public static class Methods
{
	public static DatalakeAccessState Original(DatalakeDataState state)
	{
		AccessRights defaultRule = new() { Id = 0, AccessType = AccessType.NotSet };

		#region ПОДГОТОВКА

		// Словари для быстрого доступа
		var userGroupsDict = state.UserGroupsByGuid;
		var blocksDict = state.BlocksById;
		var sourcesDict = state.SourcesById;
		var tagsDict = state.TagsById;

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
						.ToDictionary(x => x.Guid, x => new AccessRuleInfo(x.Rule.Id, x.Rule.AccessType)),
					Sources = userSources
						.ToDictionary(x => x.Id, x => new AccessRuleInfo(x.Rule.Id, x.Rule.AccessType)),
					Blocks = userBlocks
						.ToDictionary(x => x.Id, x => new AccessRuleInfo(x.Rule.Id, x.Rule.AccessType)),
					Tags = userTags
						.ToDictionary(x => x.GlobalGuid, x => new AccessRuleInfo(x.Rule.Id, x.Rule.AccessType)),
				};
			})
			.ToDictionary(x => x.Guid);

		#endregion

		var accessState = new DatalakeAccessState(userRights);
		return accessState;
	}

	public static DatalakeAccessState Optimized(DatalakeDataState state)
	{
		AccessRights defaultRule = new() { Id = 0, AccessType = AccessType.NotSet };

		#region ПОДГОТОВКА

		// Словари для быстрого доступа
		var userGroupsDict = state.UserGroupsByGuid;
		var blocksDict = state.BlocksById;
		var sourcesDict = state.SourcesById;
		var tagsDict = state.TagsById;

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
						.ToDictionary(x => x.Guid, x => new AccessRuleInfo(x.Rule.Id, x.Rule.AccessType)),
					Sources = userSources
						.ToDictionary(x => x.Id, x => new AccessRuleInfo(x.Rule.Id, x.Rule.AccessType)),
					Blocks = userBlocks
						.ToDictionary(x => x.Id, x => new AccessRuleInfo(x.Rule.Id, x.Rule.AccessType)),
					Tags = userTags
						.ToDictionary(x => x.GlobalGuid, x => new AccessRuleInfo(x.Rule.Id, x.Rule.AccessType)),
				};
			})
			.ToDictionary(x => x.Guid);

		#endregion

		var accessState = new DatalakeAccessState(userRights);
		return accessState;
	}
}

public static class Generator
{
	public static DatalakeDataState CreateTestData()
	{
		var random = new Random();
		AccessType[] rightsMap = [AccessType.NotSet, AccessType.Viewer, AccessType.Editor, AccessType.Manager, AccessType.Admin, AccessType.NoAccess];

		var blocks = GenerateBlocks(300, 20, 6, random).ToImmutableList();
		var sources = GenerateSources(10).ToImmutableList();
		var userGroups = GenerateUserGroups(20, 3, random).ToImmutableList();
		var tags = GenerateTags(5000).ToImmutableList();
		var users = GenerateUsers(500).ToImmutableList();
		var accessRights = GenerateAccessRights(random, userGroups, users, blocks, sources).ToImmutableList();
		var blockTags = GenerateBlockTags(random, blocks, tags).ToImmutableList();
		var userGroupsRelations = GenerateUserGroupRelations(random, userGroups, users).ToImmutableList();

		var state = new DatalakeDataState
		{
			Blocks = blocks,
			Sources = sources,
			UserGroups = userGroups,
			Tags = tags,
			Users = users,
			AccessRights = accessRights,
			BlockTags = blockTags,
			UserGroupRelations = userGroupsRelations,
			BlockProperties = ImmutableList<BlockProperty>.Empty,
			TagInputs = ImmutableList<TagInput>.Empty,
			Settings = new Settings()
		};

		state.InitDictionaries();
		return state;

		// Локальные функции генерации
		IEnumerable<Block> GenerateBlocks(int total, int rootCount, int maxDepth, Random rnd)
		{
			var blocks = new List<Block>();
			int id = 1;

			// Корневые блоки (20 штук)
			for (int i = 0; i < rootCount; i++)
			{
				blocks.Add(new Block { Id = id++, Name = $"RootBlock{i}", ParentId = null });
			}

			// Дочерние блоки
			while (blocks.Count < total)
			{
				var parent = blocks[rnd.Next(blocks.Count)];
				int depth = CalculateDepth(parent.Id, blocks);

				if (depth < maxDepth)
				{
					blocks.Add(new Block
					{
						Id = id++,
						Name = $"Block{id}",
						ParentId = parent.Id
					});
				}
			}
			return blocks;
		}

		int CalculateDepth(int blockId, List<Block> allBlocks)
		{
			int depth = 0;
			var current = allBlocks.First(b => b.Id == blockId);
			while (current.ParentId.HasValue)
			{
				depth++;
				current = allBlocks.First(b => b.Id == current.ParentId.Value);
			}
			return depth;
		}

		IEnumerable<Source> GenerateSources(int count)
		{
			return Enumerable.Range(1, count)
					.Select(i => new Source
					{
						Id = i,
						Name = $"Source{i}",
						Type = SourceType.Inopc,
					});
		}

		IEnumerable<UserGroup> GenerateUserGroups(int count, int maxDepth, Random rnd)
		{
			var groups = new List<UserGroup>();
			var guids = Enumerable.Range(0, count)
					.Select(_ => Guid.NewGuid())
					.ToArray();

			// Корневые группы (1/3 от общего количества)
			int rootCount = count / 3;
			for (int i = 0; i < rootCount; i++)
			{
				groups.Add(new UserGroup
				{
					Guid = guids[i],
					Name = $"RootGroup{i}",
					ParentGuid = null
				});
			}

			// Дочерние группы
			for (int i = rootCount; i < count; i++)
			{
				var parent = groups[rnd.Next(groups.Count)];
				groups.Add(new UserGroup
				{
					Guid = guids[i],
					Name = $"Group{i}",
					ParentGuid = parent.Guid
				});
			}
			return groups;
		}

		IEnumerable<Tag> GenerateTags(int count)
		{
			return Enumerable.Range(1, count)
					.Select(i => new Tag
					{
						Id = i,
						Name = $"Tag{i}",
						GlobalGuid = Guid.NewGuid(),
						Created = DateTime.Now,
						Frequency = TagFrequency.ByMinute,
						IsScaling = false,
						Type = TagType.Number,
						SourceId = random.Next(1, 11)
					});
		}

		IEnumerable<User> GenerateUsers(int count)
		{
			return Enumerable.Range(1, count)
					.Select(i => new User
					{
						Guid = Guid.NewGuid(),
						FullName = $"User{i}",
						Login = $"user{i}",
						EnergoIdGuid = Guid.NewGuid()
					});
		}

		IEnumerable<AccessRights> GenerateAccessRights(
			Random rnd,
			IEnumerable<UserGroup> userGroups,
			IEnumerable<User> users,
			IEnumerable<Block> blocks,
			IEnumerable<Source> sources)
		{
			var rights = new List<AccessRights>();
			int id = 1;

			// Глобальные права для групп (20 штук)
			foreach (var group in userGroups)
			{
				rights.Add(new AccessRights
				{
					Id = id++,
					AccessType = rightsMap[rnd.Next(0, 5)], // Viewer-Admin
					IsGlobal = true,
					UserGroupGuid = group.Guid
				});
			}

			// Глобальные права для пользователей (500 штук)
			foreach (var user in users)
			{
				rights.Add(new AccessRights
				{
					Id = id++,
					AccessType = rightsMap[rnd.Next(0, 5)],
					IsGlobal = true,
					UserGuid = user.Guid
				});
			}

			// Права на блоки для пользователей (100 штук)
			var userBlocks = blocks
					.OrderBy(_ => rnd.Next())
					.Take(100)
					.ToList();

			var usersCount = users.Count();
			foreach (var block in userBlocks)
			{
				var user = users.ElementAt(rnd.Next(usersCount));
				rights.Add(new AccessRights
				{
					Id = id++,
					AccessType = rightsMap[rnd.Next(0, 5)],
					BlockId = block.Id,
					UserGuid = user.Guid
				});
			}

			// Права на источники (2 штуки)
			var sourcesWithRights = sources
					.OrderBy(_ => rnd.Next())
					.Take(2)
					.ToList();

			foreach (var source in sourcesWithRights)
			{
				var user = users.ElementAt(rnd.Next(usersCount));
				rights.Add(new AccessRights
				{
					Id = id++,
					AccessType = rightsMap[rnd.Next(0, 5)],
					SourceId = source.Id,
					UserGuid = user.Guid
				});
			}

			return rights;
		}

		IEnumerable<BlockTag> GenerateBlockTags(Random rnd,
			IEnumerable<Block> blocks,
			IEnumerable<Tag> tags)
		{
			var relations = new List<BlockTag>();

			// Для всех тегов (минимум 1 блок)
			foreach (var tag in tags)
			{
				// Случайное количество связей (1-5)
				int linkCount = tag.Id <= 300 ? rnd.Next(2, 6) : 1;

				var selectedBlocks = blocks
						.OrderBy(_ => rnd.Next())
						.Take(linkCount)
						.ToList();

				foreach (var block in selectedBlocks)
				{
					relations.Add(new BlockTag
					{
						BlockId = block.Id,
						TagId = tag.Id
					});
				}
			}
			return relations;
		}

		IEnumerable<UserGroupRelation> GenerateUserGroupRelations(
			Random rnd,
			IEnumerable<UserGroup> userGroups,
			IEnumerable<User> users)
		{
			var relations = new List<UserGroupRelation>();
			int id = 1;

			// 20 пользователей с группами
			var usersInGroups = users
					.OrderBy(_ => rnd.Next())
					.Take(20)
					.ToList();

			foreach (var user in usersInGroups)
			{
				// 1-3 группы на пользователя
				int groupCount = rnd.Next(1, 4);
				var groups = userGroups
						.OrderBy(_ => rnd.Next())
						.Take(groupCount)
						.ToList();

				foreach (var group in groups)
				{
					relations.Add(new UserGroupRelation
					{
						Id = id++,
						UserGuid = user.Guid,
						UserGroupGuid = group.Guid,
						AccessType = rightsMap[rnd.Next(0, 5)]
					});
				}
			}
			return relations;
		}
	}
}

public class AccessStateComparer
{
	public static void CompareAccessStates(
			DatalakeAccessState originalState,
			DatalakeAccessState optimizedState)
	{
		int differences = 0;
		var stopwatch = Stopwatch.StartNew();

		// 1. Проверка количества пользователей
		if (originalState.GetAll().Count != optimizedState.GetAll().Count)
		{
			Console.WriteLine($"Количество пользователей не совпадает: " +
												$"оригинал={originalState.GetAll().Count}, " +
												$"оптимизировано={optimizedState.GetAll().Count}");
			return;
		}

		// 2. Сравнение по каждому пользователю
		foreach (var (userId, originalUser) in originalState.GetAll())
		{
			if (!optimizedState.GetAll().TryGetValue(userId, out var optimizedUser))
			{
				Console.WriteLine($"Пользователь {userId} не найден в оптимизированных данных");
				differences++;
				continue;
			}

			// 2.1. Сравнение глобальных прав
			if (originalUser.GlobalAccessType != optimizedUser.GlobalAccessType)
			{
				Console.WriteLine($"[{userId}] Глобальные права: " +
													$"оригинал={originalUser.GlobalAccessType}, " +
													$"оптимизировано={optimizedUser.GlobalAccessType}");
				differences++;
			}

			// 2.2. Сравнение прав на группы
			CompareDictionaries(
					originalUser.Groups,
					optimizedUser.Groups,
					userId,
					"Группы",
					ref differences);

			// 2.3. Сравнение прав на источники
			CompareDictionaries(
					originalUser.Sources,
					optimizedUser.Sources,
					userId,
					"Источники",
					ref differences);

			// 2.4. Сравнение прав на блоки
			CompareDictionaries(
					originalUser.Blocks,
					optimizedUser.Blocks,
					userId,
					"Блоки",
					ref differences);

			// 2.5. Сравнение прав на теги
			CompareDictionaries(
					originalUser.Tags,
					optimizedUser.Tags,
					userId,
					"Теги",
					ref differences);
		}

		// 3. Проверка тегов, отсутствующих в оригинале
		foreach (var userId in optimizedState.GetAll().Keys)
		{
			if (!originalState.GetAll().ContainsKey(userId))
			{
				Console.WriteLine($"Пользователь {userId} найден в оптимизированных данных, но отсутствует в оригинальных");
				differences++;
			}
		}

		stopwatch.Stop();
		Console.WriteLine($"\nСравнение завершено за {stopwatch.ElapsedMilliseconds} мс");
		Console.WriteLine($"Обнаружено различий: {differences}");
		Console.WriteLine(differences == 0
				? "Результаты идентичны!"
				: "ВНИМАНИЕ: Обнаружены расхождения!");
	}

	private static void CompareDictionaries<TKey>(
			IReadOnlyDictionary<TKey, AccessRuleInfo> originalDict,
			IReadOnlyDictionary<TKey, AccessRuleInfo> optimizedDict,
			Guid userId,
			string dictName,
			ref int differences)
	{
		// Проверка количества элементов
		if (originalDict.Count != optimizedDict.Count)
		{
			Console.WriteLine($"[{userId}] Количество {dictName}: " +
												$"оригинал={originalDict.Count}, " +
												$"оптимизировано={optimizedDict.Count}");
			differences++;
		}

		// Сравнение по ключам
		foreach (var key in originalDict.Keys)
		{
			if (!optimizedDict.TryGetValue(key, out var optimizedRule))
			{
				Console.WriteLine($"[{userId}] {dictName} ключ {key} не найден в оптимизированных данных");
				differences++;
				continue;
			}

			var originalRule = originalDict[key];
			if (originalRule.AccessType != optimizedRule.AccessType)
			{
				Console.WriteLine($"[{userId}] {dictName} ключ {key}: " +
													$"оригинал={originalRule.AccessType}, " +
													$"оптимизировано={optimizedRule.AccessType}");
				differences++;
			}
		}

		// Проверка лишних ключей в оптимизированной версии
		foreach (var key in optimizedDict.Keys)
		{
			if (!originalDict.ContainsKey(key))
			{
				Console.WriteLine($"[{userId}] {dictName} ключ {key} найден в оптимизированных данных, но отсутствует в оригинальных");
				differences++;
			}
		}
	}
}