using Datalake.Database.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Datalake.Database.InMemory;

/// <summary>
/// Хранилище данные о пользователях EnergoId, регулярно обновляемое
/// </summary>
public class EnergoIdUserStore
{
	private ImmutableList<EnergoIdUserView> _list = [];
	private ImmutableDictionary<Guid, EnergoIdUserView> _dict = ImmutableDictionary<Guid, EnergoIdUserView>.Empty;

	private readonly IServiceScopeFactory _scopeFactory;
	private readonly ILogger<EnergoIdUserStore> _logger;
	private readonly AsyncLock _lock = new();
	private readonly Timer _checkTimer;

	/// <summary>
	/// Конструктор
	/// </summary>
	public EnergoIdUserStore(
		IServiceScopeFactory scopeFactory,
		ILogger<EnergoIdUserStore> logger)
	{
		_scopeFactory = scopeFactory;
		_logger = logger;
		_checkTimer = new Timer(CheckForUpdates, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
	}

	/// <summary>
	/// Проверка и загрузка обновлений
	/// </summary>
	private async void CheckForUpdates(object? state)
	{
		try
		{
			using var scope = _scopeFactory.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

			// Загрузка новых данных
			var newData = await db.UsersEnergoId.ToArrayAsync();

			// Атомарное обновление
			using (await _lock.LockAsync())
			{
				_list = newData.ToImmutableList();
				_dict = _list.ToImmutableDictionary(x => x.Guid);
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Не удалось обновить данные по пользователям EnergoId");
		}
	}

	/// <summary>
	/// Получение пользователя по идентификатору
	/// </summary>
	public ImmutableDictionary<Guid, EnergoIdUserView> UsersByGuid => _dict;

	/// <summary>
	/// Получение всех пользователей
	/// </summary>
	public ImmutableList<EnergoIdUserView> Users => _list;

	/// <summary>
	/// Блокировка для атомарных операций
	/// </summary>
	private sealed class AsyncLock
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
