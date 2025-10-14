using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Infrastructure.Database;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Datalake.Inventory.Infrastructure.InMemory.Inventory;

[Singleton]
public class InventoryCache : IInventoryCache
{
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<InventoryCache> _logger;
	private readonly Channel<InventoryCacheState> _updateChannel;
	private readonly SemaphoreSlim _semaphore = new(1, 1);
	private CancellationTokenSource _processingCts = new();

	private InventoryCacheState _currentState = InventoryCacheState.Empty;

	public InventoryCache(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<InventoryCache> logger)
	{
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;

		_updateChannel = Channel.CreateBounded<InventoryCacheState>(
			new BoundedChannelOptions(100)
			{
				SingleReader = true,
				SingleWriter = false,
				FullMode = BoundedChannelFullMode.DropOldest,
			});

		_ = ProcessUpdateTasksAsync(_processingCts.Token);

		StateChanged += (_, _) => _logger.LogInformation("Кэш структуры объектов обновлен");
		StateCorrupted += (_, _) =>
		{
			_logger.LogInformation("Состояние данных повреждено, запускается восстановление");
			_ = Task.Run(RestoreAsync);
		};
	}

	public async Task RestoreAsync()
	{
		var newState = await LoadStateFromDatabaseAsync();
		await _updateChannel.Writer.WriteAsync(newState);
	}

	public async Task UpdateAsync(Func<IInventoryCacheState, IInventoryCacheState> update)
	{
		var newState = update(_currentState);
		await _updateChannel.Writer.WriteAsync((InventoryCacheState)newState);
	}

	public IInventoryCacheState State => _currentState;

	public event EventHandler<IInventoryCacheState>? StateChanged;

	public event EventHandler<int>? StateCorrupted;


	private async Task ProcessUpdateTasksAsync(CancellationToken ct)
	{
		await foreach (var newState in _updateChannel.Reader.ReadAllAsync(ct))
		{
			await ProcessUpdateTaskAsync(newState);
		}
	}

	private async Task ProcessUpdateTaskAsync(InventoryCacheState newState)
	{
		_logger.LogDebug("Вызвано обновление состояния кэша структуры объектов версии {version}", newState.Version);

		await _semaphore.WaitAsync();

		try
		{
			if (newState.Version <= _currentState.Version)
			{
				_logger.LogDebug("Обновление состояния кэша структуры объектов версии {version} прекращено - текущая версия новее: {current}", newState.Version, _currentState.Version);
			}
			else
			{
				Interlocked.Exchange(ref _currentState, newState);

				_logger.LogDebug("Выполнено обновление состояния кэша структуры объектов версии {version}", newState.Version);

				_ = Task.Run(() => StateChanged?.Invoke(this, _currentState));
			}
		}
		catch (Exception e)
		{
			_logger.LogWarning("Ошибка при обновлении состояния кэша структуры объектов версии {version}: {message}", newState.Version, e.Message);

			_ = Task.Run(() => StateCorrupted?.Invoke(this, 0));
		}
		finally
		{
			_semaphore.Release();
		}
	}

	private async Task<InventoryCacheState> LoadStateFromDatabaseAsync()
	{
		return await Measures.Measure(async () =>
		{
			using var scope = _serviceScopeFactory.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

			// коллекции основных объектов
			var blocks = await context.Blocks.Where(x => !x.IsDeleted).AsNoTracking().ToArrayAsync();
			var sources = await context.Sources.Where(x => !x.IsDeleted).AsNoTracking().ToArrayAsync();
			var tags = await context.Tags.Where(x => !x.IsDeleted).AsNoTracking().ToArrayAsync();
			var users = await context.Users.Where(x => !x.IsDeleted).AsNoTracking().ToArrayAsync();
			var userGroups = await context.UserGroups.Where(x => !x.IsDeleted).AsNoTracking().ToArrayAsync();

			// коллекции связующих объектов
			var blockTags = await context.BlockTags.AsNoTracking().ToArrayAsync();
			var userGroupRelations = await context.UserGroupRelations.AsNoTracking().ToArrayAsync();

			// коллекция правил
			var accessRules = await context.AccessRules.AsNoTracking().ToArrayAsync();

			return InventoryCacheState.Create(
				accessRules: accessRules,
				blocks: blocks,
				blockTags: blockTags,
				sources: sources,
				tags: tags,
				users: users,
				userGroups: userGroups,
				userGroupRelations: userGroupRelations);

		}, _logger, nameof(LoadStateFromDatabaseAsync));
	}
}
