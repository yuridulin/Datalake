using Datalake.InventoryService.Infrastructure.Database;
using Datalake.PrivateApi.Utils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Datalake.InventoryService.Infrastructure.Cache.Inventory;

public class InventoryCacheStore : IInventoryCache
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<InventoryCacheStore> _logger;
	private readonly SemaphoreSlim _semaphore = new(1, 1);
	private InventoryState _currentState = new()
	{
		AccessRights = [],
		BlockProperties = [],
		Blocks = [],
		BlockTags = [],
		Sources = [],
		TagInputs = [],
		Tags = [],
		UserGroupRelations = [],
		UserGroups = [],
		Users = [],
	};

	public InventoryCacheStore(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<InventoryCacheStore> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;

		StateChanged += (_, _) => _logger.LogInformation("Состояние данных изменено");
		StateCorrupted += (_, _) => Task.Run(RestoreAsync);

		// начальное заполнение кэша будет запущено, когда все будет готов, извне
	}

	public async Task RestoreAsync()
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<InventoryEfContext>();

		var newState = await LoadStateFromDatabaseAsync(db);

		await UpdateAsync((_) => newState);

		_logger.LogInformation("Состояние данных перезагружено");
	}

	public async Task<InventoryState> UpdateAsync(Func<InventoryState, InventoryState> update)
	{
		InventoryState newState;

		await _semaphore.WaitAsync();

		try
		{
			newState = UpdateStateWithinLock(update);
		}
		finally
		{
			_semaphore.Release();
		}

		return newState;
	}

	public InventoryState State => _currentState;

	public event EventHandler<InventoryState>? StateChanged;

	public event EventHandler<int>? StateCorrupted;


	private async Task<InventoryState> LoadStateFromDatabaseAsync(InventoryEfContext context)
	{
		return await Measures.Measure(async () =>
		{
			var accessRights = await context.AccessRights.AsNoTracking().ToArrayAsync();
			var blocks = await context.Blocks.AsNoTracking().ToArrayAsync();
			var blockProperties = await context.BlockProperties.AsNoTracking().ToArrayAsync();
			var blockTags = await context.BlockTags.AsNoTracking().ToArrayAsync();
			var sources = await context.Sources.AsNoTracking().ToArrayAsync();
			var tags = await context.Tags.AsNoTracking().ToArrayAsync();
			var tagInputs = await context.TagInputs.AsNoTracking().ToArrayAsync();
			var users = await context.Users.AsNoTracking().ToArrayAsync();
			var userGroups = await context.UserGroups.AsNoTracking().ToArrayAsync();
			var userGroupRelations = await context.UserGroupRelations.AsNoTracking().ToArrayAsync();

			var newState = new InventoryState
			{
				AccessRights = accessRights.ToImmutableList(),
				Blocks = blocks.ToImmutableList(),
				BlockProperties = blockProperties.ToImmutableList(),
				BlockTags = blockTags.ToImmutableList(),
				Sources = sources.ToImmutableList(),
				Tags = tags.ToImmutableList(),
				TagInputs = tagInputs.ToImmutableList(),
				Users = users.ToImmutableList(),
				UserGroups = userGroups.ToImmutableList(),
				UserGroupRelations = userGroupRelations.ToImmutableList(),
			};
			return newState;
		}, _logger, nameof(LoadStateFromDatabaseAsync));
	}

	private InventoryState UpdateStateWithinLock(Func<InventoryState, InventoryState> update)
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
}
