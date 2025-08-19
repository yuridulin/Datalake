using Datalake.Database.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Datalake.Database.InMemory;

///<summary>
/// Хранилище данные о пользователях EnergoId, регулярно обновляемое
///</summary>
public sealed class DatalakeEnergoIdStore(
	IServiceScopeFactory scopeFactory,
	ILogger<DatalakeEnergoIdStore> logger) : BackgroundService
{
	/// <summary>
	/// Получение всех пользователей
	/// </summary>
	public ImmutableList<EnergoIdUserView> Users => Volatile.Read(ref _state).List;

	/// <summary>
	/// Получение пользователя по идентификатору
	/// </summary>
	public ImmutableDictionary<Guid, EnergoIdUserView> UsersByGuid => Volatile.Read(ref _state).Dict;

	/// <summary>
	/// Публичный ручной триггер
	/// </summary>
	public Task ForceRefreshAsync(CancellationToken ct = default) => SafeRefreshAsync(ct);


	private readonly SemaphoreSlim _refreshGate = new(1, 1);
	static readonly TimeSpan interval = TimeSpan.FromMinutes(1);
	private EnergoIdState _state = EnergoIdState.Empty;

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
			var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

			// Запрос к view
			var data = await db.UsersEnergoId.ToListAsync(ct);

			var dict = data.ToImmutableDictionary(x => x.Guid);
			var list = dict.Values.ToImmutableList();

			var newSnapshot = new EnergoIdState(list, dict);
			Volatile.Write(ref _state, newSnapshot);

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

	private sealed record EnergoIdState(
		ImmutableList<EnergoIdUserView> List,
		ImmutableDictionary<Guid, EnergoIdUserView> Dict)
	{
		public static readonly EnergoIdState Empty = new([], ImmutableDictionary<Guid, EnergoIdUserView>.Empty);
	}
}
