using Datalake.InventoryService.Infrastructure.Database;
using Datalake.InventoryService.Infrastructure.Database.Views;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Datalake.InventoryService.Infrastructure.Cache.EnergoId;

///<summary>
/// Хранилище данные о пользователях EnergoId, регулярно обновляемое
///</summary>
public sealed class EnergoIdCacheStore(
	IServiceScopeFactory scopeFactory,
	ILogger<EnergoIdCacheStore> logger) : BackgroundService, IEnergoIdCache
{
	/// <summary>
	/// Текущее состояние
	/// </summary>
	public EnergoIdState State => Volatile.Read(ref _state);

	/// <summary>
	/// Публичный ручной триггер
	/// </summary>
	public Task UpdateAsync(CancellationToken ct = default) => SafeRefreshAsync(ct);


	private readonly SemaphoreSlim _refreshGate = new(1, 1);
	static readonly TimeSpan interval = TimeSpan.FromMinutes(1);
	private EnergoIdState _state = new() { Users = [], UsersByGuid = ImmutableDictionary<Guid, EnergoIdUserView>.Empty };

	/// <inheritdoc/>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var timer = new PeriodicTimer(interval);

		try
		{
			// Первичная загрузка сразу
			await SafeRefreshAsync(stoppingToken);

			// Периодические обновления
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				await SafeRefreshAsync(stoppingToken);
			}
		}
		catch (OperationCanceledException) { /* нормальное завершение */ }
	}

	private async Task SafeRefreshAsync(CancellationToken ct)
	{
		if (!await _refreshGate.WaitAsync(0, ct))
		{
			logger.LogDebug("Пропуск обновления EnergoId: предыдущее ещё выполняется");
			return;
		}

		try
		{
			logger.LogDebug("Начато обновление EnergoId");

			using var scope = scopeFactory.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<InventoryEfContext>();

			var data = await db.UsersEnergoId.AsNoTracking().ToListAsync(ct);

			var dict = data.ToImmutableDictionary(x => x.Guid);
			var list = dict.Values.ToImmutableList();

			var newState = new EnergoIdState { Users = list, UsersByGuid = dict, };

			Volatile.Write(ref _state, newState);

			logger.LogDebug("Выполнено обновление EnergoId");
		}
		catch (OperationCanceledException) { throw; }
		catch (Exception ex)
		{
			logger.LogError(ex, "Сбой обновления EnergoId");
		}
		finally
		{
			_refreshGate.Release();
		}
	}
}
