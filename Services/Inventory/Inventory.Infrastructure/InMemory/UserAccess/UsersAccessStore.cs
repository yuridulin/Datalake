using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Models;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Datalake.Inventory.Infrastructure.InMemory.UserAccess;

[Singleton]
public class UsersAccessStore : IUsersAccessStore
{
	private readonly ILogger<UsersAccessStore> _logger;
	private readonly Channel<UsersAccessDto> _rebuildChannel;
	private readonly SemaphoreSlim semaphore = new(1, 1);

	private UserAccessCacheState _currentState = UserAccessCacheState.Empty;
	private CancellationTokenSource _processingCts = new();

	public UsersAccessStore(ILogger<UsersAccessStore> logger)
	{
		_logger = logger;

		_rebuildChannel = Channel.CreateBounded<UsersAccessDto>(
			new BoundedChannelOptions(1) // Только последнее состояние
			{
				SingleReader = true,
				SingleWriter = false,
				FullMode = BoundedChannelFullMode.DropOldest
			});

		_ = ProcessSetTasksAsync(_processingCts.Token);

		StateChanged += (_, _) => _logger.LogInformation("Кэш прав доступа обновлен");
	}

	public async Task SetAsync(UsersAccessDto usersAccess)
	{
		await _rebuildChannel.Writer.WriteAsync(usersAccess);
	}

	public IUserAccessState State => _currentState;

	public event EventHandler<IUserAccessState>? StateChanged;


	private async Task ProcessSetTasksAsync(CancellationToken ct)
	{
		await foreach (var newState in _rebuildChannel.Reader.ReadAllAsync(ct))
		{
			await ProcessSetTaskAsync(newState);
		}
	}

	private async Task ProcessSetTaskAsync(UsersAccessDto usersAccess)
	{
		_logger.LogDebug("Вызвано обновление состояния кэша прав доступа версии {version}", usersAccess.Version);

		await semaphore.WaitAsync();

		try
		{
			if (usersAccess.Version <= _currentState.Version)
			{
				_logger.LogDebug("Обновление состояния кэша прав доступа версии {version} прекращено - текущая версия новее: {current}", usersAccess.Version, _currentState.Version);
			}
			else
			{
				var newState = new UserAccessCacheState(usersAccess);

				if (newState == _currentState)
				{
					_logger.LogInformation("Обновление кэша прав доступа завершено для версии {version}, изменений нет", usersAccess.Version);
				}
				else
				{
					Interlocked.Exchange(ref _currentState, newState);

					_logger.LogInformation("Обновление кэша прав доступа завершено для версии {version}, есть изменения", usersAccess.Version);

					_ = Task.Run(() => StateChanged?.Invoke(this, newState));
				}
			}
		}
		catch (Exception e)
		{
			_logger.LogError(e, "Ошибка обновления кэша прав доступа для версии {version}", usersAccess.Version);

			// нужна ли функция перечитывания из БД?
			// наверное нет, это ведь зависимые данные, в БД только копия, а не оригинал
		}
		finally
		{
			semaphore.Release();
		}
	}
}
