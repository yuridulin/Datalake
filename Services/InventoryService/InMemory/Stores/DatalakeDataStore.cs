using Datalake.InventoryService.Database;
using Datalake.InventoryService.InMemory.Models;
using Datalake.PrivateApi.Utils;
using Datalake.PublicApi.Exceptions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Datalake.InventoryService.InMemory.Stores;

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
			Settings = new(DateTime.MinValue),
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

		StateChanged += (_, _) => _logger.LogInformation("Состояние данных изменено");
		StateCorrupted += (_, _) => Task.Run(ReloadStateAsync);

		//_ = LoadStateFromDatabaseAsync(); // Инициализатор БД сделает это сам
	}

	public async Task ReloadStateAsync()
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<InventoryEfContext>();

		var newState = await LoadStateFromDatabaseAsync(db);

		using (await AcquireWriteLockAsync())
		{
			UpdateStateWithinLock(_ => newState);

			_logger.LogInformation("Состояние данных перезагружено");
		}
	}

	private async Task<DatalakeDataState> LoadStateFromDatabaseAsync(InventoryEfContext db)
	{
		return await Measures.Measure(async () =>
		{
			var accessRights = await db.AccessRights.AsNoTracking().ToArrayAsync();
			var blocks = await db.Blocks.AsNoTracking().ToArrayAsync();
			var blockProperties = await db.BlockProperties.AsNoTracking().ToArrayAsync();
			var blockTags = await db.BlockTags.AsNoTracking().ToArrayAsync();
			var sources = await db.Sources.AsNoTracking().ToArrayAsync();
			var settings = await db.Settings.AsNoTracking().ToArrayAsync();
			var tags = await db.Tags.AsNoTracking().ToArrayAsync();
			var tagInputs = await db.TagInputs.AsNoTracking().ToArrayAsync();
			var users = await db.Users.AsNoTracking().ToArrayAsync();
			var userGroups = await db.UserGroups.AsNoTracking().ToArrayAsync();
			var userGroupRelations = await db.UserGroupRelations.AsNoTracking().ToArrayAsync();
			var userSessions = await db.UserSessions.AsNoTracking().ToArrayAsync();

			var newState = new DatalakeDataState
			{
				AccessRights = accessRights.ToImmutableList(),
				Blocks = blocks.ToImmutableList(),
				BlockProperties = blockProperties.ToImmutableList(),
				BlockTags = blockTags.ToImmutableList(),
				Settings = settings.FirstOrDefault() ?? new(DateTime.MinValue),
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

	public async Task<TResult> ExecuteAtomicUpdateAsync<TResult>(
		Func<DatalakeDataState, InventoryEfContext, (TResult result, DatalakeDataState newState, Func<Task> databaseUpdate)> updateLogic)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<InventoryEfContext>();

		using (await AcquireWriteLockAsync())
		{
			using var transaction = await db.Database.BeginTransactionAsync();

			try
			{
				var (result, newState, databaseUpdate) = updateLogic(_currentState, db);

				await databaseUpdate();
				await db.SaveChangesAsync();
				await transaction.CommitAsync();

				UpdateStateWithinLock(_ => newState);

				return result;
			}
			catch (DbUpdateException ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Ошибка БД при атомарном обновлении");
				throw new DatabaseException("Ошибка сохранения в БД", ex);
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				_logger.LogError(ex, "Ошибка при атомарном обновлении");
				throw;
			}
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
