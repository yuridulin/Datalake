using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Infrastructure.Interfaces;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Datalake.Inventory.Infrastructure.InMemory.UserAccess;

[Singleton]
public class UserAccessCache : IUserAccessCache
{
	private readonly ILogger<UserAccessCache> _logger;
	private readonly IUserAccessStateFactory _accessStateFactory;
	private readonly Channel<IInventoryCacheState> _rebuildChannel;
	private UserAccessState _state = UserAccessState.Empty;
	private CancellationTokenSource _processingCts = new();
	private long _lastProcessingDataVersion;

	public UserAccessCache(
		IInventoryCache inventoryCache,
		IUserAccessStateFactory accessStateFactory,
		ILogger<UserAccessCache> logger)
	{
		_logger = logger;
		_accessStateFactory = accessStateFactory;

		_rebuildChannel = Channel.CreateBounded<IInventoryCacheState>(
			new BoundedChannelOptions(1) // Только последнее состояние
			{
				SingleReader = true,
				SingleWriter = false,
				FullMode = BoundedChannelFullMode.DropOldest
			});

		_ = ProcessRebuildsAsync(_processingCts.Token);

		inventoryCache.StateChanged += async (_, newState) =>
		{
			if (newState.Version > Interlocked.Read(ref _lastProcessingDataVersion))
			{
				await _rebuildChannel.Writer.WriteAsync(newState);
			}
		};
	}

	public IUserAccessCacheState State => _state;

	public event EventHandler<IUserAccessCacheState>? StateChanged;

	private async Task ProcessRebuildsAsync(CancellationToken ct)
	{
		await foreach (var newState in _rebuildChannel.Reader.ReadAllAsync(ct))
		{
			await RebuildAsync(newState);
		}
	}

	private async Task RebuildAsync(IInventoryCacheState newDataState)
	{
		try
		{
			// Гарантируем, что только одна задача выполняется одновременно
			var newState = await Task.Run(() =>
					Measures.Measure(() => _accessStateFactory.Create(newDataState), _logger, $"{nameof(UserAccessCache)}"));

			Interlocked.Exchange(ref _state, newState);
			Interlocked.Exchange(ref _lastProcessingDataVersion, newDataState.Version);

			_logger.LogInformation("Обновление кэша прав доступа завершено для версии {Version}", newDataState.Version);

			_ = Task.Run(() => StateChanged?.Invoke(this, newState));
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Ошибка обновления кэша прав доступа для версии {Version}", newDataState.Version);
		}
	}
}