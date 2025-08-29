using Datalake.Database.Attributes;
using Datalake.Database.InMemory.Models;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using LinqToDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Datalake.Database.InMemory.Stores;

#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена

public class DatalakeDataStore
{
	public DatalakeDataStore(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<DatalakeDataStore> logger)
	{
		var initialState = new DatalakeDataState
		{
			AccessRights = [],
			BlockProperties = [],
			Blocks = [],
			BlockTags = [],
			Settings = new(),
			Sources = [],
			TagInputs = [],
			Tags = [],
			UserGroupRelations = [],
			UserGroups = [],
			Users = [],
			UserSessions = [],
		};
		initialState.InitDictionaries();
		_currentState = initialState;

		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;

		StateChanged += (_, _) => _logger.LogInformation("Стейт изменён");
		StateCorrupted += (_, _) => Task.Run(ReloadStateAsync);

		//_ = LoadStateFromDatabaseAsync(); // Инициализатор БД сделает это сам
	}

	public async Task ReloadStateAsync()
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		var newState = await LoadStateFromDatabaseAsync(db);

		using (await AcquireWriteLockAsync())
		{
			UpdateStateWithinLock(_ => newState);

			await db.InsertAsync(new Log
			{
				Category = LogCategory.Core,
				Type = LogType.Success,
				Text = "Состояние данных перезагружено",
			});
		}
	}

	private async Task<DatalakeDataState> LoadStateFromDatabaseAsync(DatalakeContext db)
	{
		return await Measures.Measure(async () =>
		{
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
			var userSessions = await db.UserSessions.ToArrayAsync();

			var newState = new DatalakeDataState
			{
				AccessRights = accessRights.ToImmutableList(),
				Blocks = blocks.ToImmutableList(),
				BlockProperties = blockProperties.ToImmutableList(),
				BlockTags = blockTags.ToImmutableList(),
				Settings = settings.FirstOrDefault() ?? new(),
				Sources = sources.ToImmutableList(),
				Tags = tags.ToImmutableList(),
				TagInputs = tagInputs.ToImmutableList(),
				Users = users.ToImmutableList(),
				UserGroups = userGroups.ToImmutableList(),
				UserGroupRelations = userGroupRelations.ToImmutableList(),
				UserSessions = userSessions.ToImmutableList(),
			};
			return newState;
		}, _logger, nameof(LoadStateFromDatabaseAsync));
	}

	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<DatalakeDataStore> _logger;
	private readonly AsyncLock _writeLock = new();
	private DatalakeDataState _currentState;

	public DatalakeDataState State => _currentState;

	public async Task<IDisposable> AcquireWriteLockAsync() => await _writeLock.LockAsync();

	/// <summary>
	/// Создание нового стейта на основе предыдущего<br />
	/// Этот метод должен вызываться только внутри активной блокировки <see cref="AcquireWriteLockAsync"/>
	/// </summary>
	/// <param name="update">Текущий стейт</param>
	public DatalakeDataState UpdateStateWithinLock(Func<DatalakeDataState, DatalakeDataState> update)
	{
		try
		{
			var newState = update(_currentState);
			newState.InitDictionaries();

			_currentState = newState;

			StateChanged?.Invoke(this, _currentState);

			return newState;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при изменении стейта");
			StateCorrupted?.Invoke(this, 0);

			return _currentState;
		}
	}

	public event EventHandler<DatalakeDataState>? StateChanged;

	public event EventHandler<int>? StateCorrupted;

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
}

#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена