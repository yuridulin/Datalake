using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Datalake.Database.Functions;
using Datalake.Database.InMemory.Models;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using System.Collections.Immutable;

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
		return AccessBuild.ComputateRightsFromState(state);
	}

	public static DatalakeAccessState Optimized(DatalakeDataState state)
	{
		var info = OptimizedFunctions.ComputeMeta(state);
		var groups = OptimizedFunctions.ComputeUserGroups(state, info);
		var users = OptimizedFunctions.ComputeUsers(state, info, groups);
		var accessState = new DatalakeAccessState(users);

		return accessState;
	}
}

public static class Generator
{
	public static DatalakeDataState CreateTestData(int blocksCount = 300, int tagsCount = 2000, int usersCount = 400)
	{
		var random = new Random();
		AccessType[] rightsMap = [AccessType.NotSet, AccessType.Viewer, AccessType.Editor, AccessType.Manager, AccessType.Admin, AccessType.NoAccess];

		var blocks = GenerateBlocks(total: blocksCount, rootCount: 20, maxDepth: 6, random).ToImmutableList();
		var sources = GenerateSources(count: 10).ToImmutableList();
		var userGroups = GenerateUserGroups(count: 150, maxDepth: 5, random).ToImmutableList();
		var tags = GenerateTags(count: tagsCount).ToImmutableList();
		var users = GenerateUsers(count: usersCount).ToImmutableList();

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
			var half = users.Count() % 2;
			var usersInGroups = users
				.OrderBy(_ => rnd.Next())
				.Take(half)
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
		DatalakeDataState state,
		DatalakeAccessState originalAccessState,
		DatalakeAccessState optimizedAccessState)
	{
		foreach (var user in state.Users)
		{
			var originalUser = originalAccessState.Get(user.Guid);
			var optimizedUser = optimizedAccessState.Get(user.Guid);

			List<string> messages = [];

			if (originalUser.GlobalAccessType != optimizedUser.GlobalAccessType)
				messages.Add($"Глобальный доступ: {originalUser.GlobalAccessType} != {optimizedUser.GlobalAccessType}");

			foreach (var group in state.UserGroups)
			{
				var original = originalUser.Groups[group.Guid];
				var optimized = optimizedUser.Groups[group.Guid];

				if (original.AccessType != optimized.AccessType)
					messages.Add($"Группа {group.Guid}: {original.AccessType} != {optimized.AccessType}");
			}

			foreach (var source in state.Sources)
			{
				var original = originalUser.Sources[source.Id];
				var optimized = optimizedUser.Sources[source.Id];

				if (original.AccessType != optimized.AccessType)
					messages.Add($"Источник {source.Id}: {original.AccessType} != {optimized.AccessType}");
			}

			foreach (var block in state.Blocks)
			{
				var original = originalUser.Blocks[block.Id];
				var optimized = optimizedUser.Blocks[block.Id];

				if (original.AccessType != optimized.AccessType)
					messages.Add($"Блок {block.Id}: {original.AccessType} != {optimized.AccessType}");
			}

			foreach (var tag in state.Tags)
			{
				var original = originalUser.Tags[tag.GlobalGuid];
				var optimized = optimizedUser.Tags[tag.GlobalGuid];

				if (original.AccessType != optimized.AccessType)
					messages.Add($"Тег {tag.GlobalGuid}: {original.AccessType} != {optimized.AccessType}");
			}

			if (messages.Count > 0)
			{
				Console.WriteLine($"Пользователь {user.Guid}: {originalUser.GlobalAccessType}");
				Console.WriteLine(string.Join("\n", messages));
			}
		}
	}
}