using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Infrastructure.Database;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Infrastructure.Cache.Inventory;

[Singleton]
public class InventoryCache : IInventoryCache
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<InventoryCache> _logger;
	private readonly SemaphoreSlim _semaphore = new(1, 1);
	private InventoryState _currentState = InventoryState.Empty;

	public InventoryCache(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<InventoryCache> logger)
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
		var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

		var newState = await LoadStateFromDatabaseAsync(db);

		await UpdateAsync((_) => newState);

		_logger.LogInformation("Состояние данных перезагружено");
	}

	public async Task<IInventoryCacheState> UpdateAsync(Func<IInventoryCacheState, IInventoryCacheState> update)
	{
		IInventoryCacheState newState;

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

	public IInventoryCacheState State => _currentState;

	public event EventHandler<IInventoryCacheState>? StateChanged;

	public event EventHandler<int>? StateCorrupted;


	private async Task<IInventoryCacheState> LoadStateFromDatabaseAsync(InventoryDbContext context)
	{
		return await Measures.Measure(async () =>
		{
			var accessRules = await context.AccessRights.AsNoTracking().ToArrayAsync();
			var blocks = await context.Blocks.AsNoTracking().ToArrayAsync();
			var blockProperties = await context.BlockProperties.AsNoTracking().ToArrayAsync();
			var blockTags = await context.BlockTags.AsNoTracking().ToArrayAsync();
			var sources = await context.Sources.AsNoTracking().ToArrayAsync();
			var tags = await context.Tags.AsNoTracking().ToArrayAsync();
			var tagInputs = await context.TagInputs.AsNoTracking().ToArrayAsync();
			//var tagThresholds = await context.TagThresholds.AsNoTracking().ToArrayAsync();
			var users = await context.Users.AsNoTracking().ToArrayAsync();
			var userGroups = await context.UserGroups.AsNoTracking().ToArrayAsync();
			var userGroupRelations = await context.UserGroupRelations.AsNoTracking().ToArrayAsync();

			return InventoryState.Create(
				accessRules: accessRules,
				blocks: blocks,
				blockProperties: blockProperties,
				blockTags: blockTags,
				sources: sources,
				tags: tags,
				tagInputs: tagInputs,
				//tagThresholds: tagThresholds,
				users: users,
				userGroups: userGroups,
				userGroupRelations: userGroupRelations);

		}, _logger, nameof(LoadStateFromDatabaseAsync));
	}

	private InventoryState UpdateStateWithinLock(Func<IInventoryCacheState, IInventoryCacheState> update)
	{
		try
		{
			var newState = update(_currentState);
			if (newState.Version != _currentState.Version)
			{
				_currentState = (InventoryState)newState;
				StateChanged?.Invoke(this, _currentState);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Ошибка при изменении стейта");
			StateCorrupted?.Invoke(this, 0);
		}

		return _currentState;
	}
}
