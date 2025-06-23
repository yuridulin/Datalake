using Datalake.Database.Tables;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Datalake.Database.InMemory;

#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

public class DatalakeDataStore
{
	public DatalakeDataStore(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<DatalakeDataStore> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;

		StateChanged += (_, _) => _logger.LogInformation("Стейт перезагружен");
		StateCorrupted += (_, _) => Task.Run(LoadStateFromDatabaseAsync);

		_ = LoadStateFromDatabaseAsync();
	}

	public async Task LoadStateFromDatabaseAsync()
	{
		using (await AcquireWriteLockAsync())
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

			var t = Stopwatch.StartNew();

			var accessRights = await db.AccessRights.ToArrayAsync();
			var blocks = await db.Blocks.ToArrayAsync();
			var blockProperties = await db.BlockProperties.ToArrayAsync();
			var blockTags = await db.BlockTags.ToArrayAsync();
			var sources = await db.Sources.ToArrayAsync();
			var settings = await db.Settings.ToArrayAsync();
			var tags = await db.Tags.ToArrayAsync();
			var tagInputs = await db.TagInputs.ToArrayAsync();
			var users = await db.Users.ToArrayAsync();
			var userGroups = await db.UserGroups.ToArrayAsync();
			var userGroupRelations = await db.UserGroupRelations.ToArrayAsync();

			if (settings.Length == 0)
			{
				await db.EnsureDataCreatedAsync();
				settings = await db.Settings.ToArrayAsync();
			}

			t.Stop();
			_logger.LogInformation("Загрузка БД: {ms}", t.Elapsed.TotalMilliseconds);

			var newState = new DatalakeDataState
			{
				AccessRights = accessRights.ToImmutableList(),
				Blocks = blocks.ToImmutableList(),
				BlockProperties = blockProperties.ToImmutableList(),
				BlockTags = blockTags.ToImmutableList(),
				Settings = settings[0],
				Sources = sources.ToImmutableList(),
				Tags = tags.ToImmutableList(),
				TagInputs = tagInputs.ToImmutableList(),
				Users = users.ToImmutableList(),
				UserGroups = userGroups.ToImmutableList(),
				UserGroupRelations = userGroupRelations.ToImmutableList(),
			};

			UpdateStateWithinLock(_ => newState);
		}
	}

	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<DatalakeDataStore> _logger;
	private readonly AsyncLock _writeLock = new();
	private DatalakeDataState _currentState = new();

	public DatalakeDataState State => _currentState;
	
	public async Task<IDisposable> AcquireWriteLockAsync() => await _writeLock.LockAsync();

	/// <summary>
	/// Создание нового стейта на основе предыдущего<br />
	/// Этот метод должен вызываться только внутри активной блокировки <see cref="AcquireWriteLockAsync"/>
	/// </summary>
	/// <param name="update">Текущий стейт</param>
	public void UpdateStateWithinLock(Func<DatalakeDataState, DatalakeDataState> update)
	{
		try
		{
			var newState = update(_currentState);
			newState.InitDictionaries();

			_currentState = newState;

			StateChanged?.Invoke(this, _currentState);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при изменении стейта");
			StateCorrupted?.Invoke(this, 0);
		}
	}

	public event EventHandler<DatalakeDataState>? StateChanged;

	public event EventHandler<int>? StateCorrupted;

}

public sealed class AsyncLock
{
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public async Task<IDisposable> LockAsync()
	{
		await _semaphore.WaitAsync();
		return new Releaser(_semaphore);
	}

	private sealed class Releaser(SemaphoreSlim semaphore) : IDisposable
	{
		public void Dispose() => semaphore.Release();
	}
}

public struct DatalakeDataState
{
	public long Version { get; private set; }

	// Таблицы

	public ImmutableList<AccessRights> AccessRights { get; init; }

	public ImmutableList<Block> Blocks { get; init; }

	public ImmutableList<BlockProperty> BlockProperties { get; init; }

	public ImmutableList<BlockTag> BlockTags { get; init; }

	public ImmutableList<Source> Sources { get; init; }

	public Settings Settings { get; init; }

	public ImmutableList<Tag> Tags { get; init; }

	public ImmutableList<TagInput> TagInputs { get; init; }

	public ImmutableList<User> Users { get; init; }

	public ImmutableList<UserGroup> UserGroups { get; init; }

	public ImmutableList<UserGroupRelation> UserGroupRelations { get; init; }

	// Словари

	public void InitDictionaries()
	{
		BlocksById = Blocks.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		SourcesById = Sources.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		TagsByGuid = Tags.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.GlobalGuid);
		TagsById = Tags.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		UsersByGuid = Users.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Guid);
		UserGroupsByGuid = UserGroups.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Guid);

		Version = DateTime.UtcNow.Ticks;
	}

	public ImmutableDictionary<int, Block> BlocksById { get; private set; }

	public ImmutableDictionary<int, Source> SourcesById { get; private set; }

	public ImmutableDictionary<Guid, Tag> TagsByGuid { get; private set; }

	public ImmutableDictionary<int, Tag> TagsById { get; private set; }

	public ImmutableDictionary<Guid, User> UsersByGuid { get; private set; }

	public ImmutableDictionary<Guid, UserGroup> UserGroupsByGuid { get; private set; }
}

#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена