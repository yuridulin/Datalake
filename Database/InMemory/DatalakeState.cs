using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;

namespace Datalake.Database.InMemory;

#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

public class DatalakeStateHolder
{
	public DatalakeStateHolder(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<DatalakeStateHolder> logger)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		_logger = logger;

		StateChanged += (_, _) => _logger.LogInformation("Стейт перезагружен");

		Initialize(db).Wait();
	}

	public async Task Initialize(DatalakeContext db)
	{
		var t = Stopwatch.StartNew();

		var accessRights = await db.AccessRights.ToArrayAsync();
		var blocks = await db.Blocks.ToArrayAsync();
		var blockProperties = await db.BlockProperties.ToArrayAsync();
		var blockTags = await db.BlockTags.ToArrayAsync();
		var sources = await db.Sources.ToArrayAsync();
		var tags = await db.Tags.ToArrayAsync();
		var tagInputs = await db.TagInputs.ToArrayAsync();
		var users = await db.Users.ToArrayAsync();
		var userGroups = await db.UserGroups.ToArrayAsync();
		var userGroupRelations = await db.UserGroupRelations.ToArrayAsync();

		t.Stop();
		_logger.LogInformation("Загрузка БД: {ms}", t.Elapsed.TotalMilliseconds);

		var newState = new DatalakeState
		{
			AccessRights = accessRights.ToImmutableArray(),
			Blocks = blocks.ToImmutableArray(),
			BlockProperties = blockProperties.ToImmutableArray(),
			BlockTags = blockTags.ToImmutableArray(),
			Sources = sources.ToImmutableArray(),
			Tags = tags.ToImmutableArray(),
			TagInputs = tagInputs.ToImmutableArray(),
			Users = users.ToImmutableArray(),
			UserGroups = userGroups.ToImmutableArray(),
			UserGroupRelations = userGroupRelations.ToImmutableArray(),
		};

		lock (_lock)
		{
			_currentState = newState;
		}

		StateChanged?.Invoke(this, _currentState);
	}

	private DatalakeState _currentState = new();

	private readonly ILogger<DatalakeStateHolder> _logger;

	private readonly object _lock = new();

	public DatalakeState CurrentState => _currentState;

	public void UpdateState(Func<DatalakeState, DatalakeState> update)
	{
		var newState = update(_currentState);

		lock (_lock)
		{
			_currentState = newState;
		}

		StateChanged?.Invoke(this, _currentState);
	}

	public event EventHandler<DatalakeState>? StateChanged;
}

public readonly struct DatalakeState
{
	public long Version { get; init; }

	#region Таблицы

	public ImmutableArray<AccessRights> AccessRights { get; init; }

	public ImmutableArray<Block> Blocks { get; init; }

	public ImmutableArray<BlockProperty> BlockProperties { get; init; }

	public ImmutableArray<BlockTag> BlockTags { get; init; }

	public ImmutableArray<Source> Sources { get; init; }

	public ImmutableArray<Tag> Tags { get; init; }

	public ImmutableArray<TagInput> TagInputs { get; init; }

	public ImmutableArray<User> Users { get; init; }

	public ImmutableArray<UserGroup> UserGroups { get; init; }

	public ImmutableArray<UserGroupRelation> UserGroupRelations { get; init; }

	#endregion
}

#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена