using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Datalake.Inventory.Infrastructure.InMemory.EnergoId;

///<summary>
/// Хранилище данные о пользователях EnergoId, регулярно обновляемое.
/// Это inMemory, потому что представление большое и долго грузится
///</summary>
[Singleton]
public sealed class EnergoIdCache(
	IServiceScopeFactory scopeFactory,
	ILogger<EnergoIdCache> logger) : BackgroundService, IEnergoIdCache
{
	#region Периодическое обновление

	private TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
	private readonly SemaphoreSlim _refreshGate = new(1, 1);
	static readonly TimeSpan interval = TimeSpan.FromMinutes(1);

	public void SetReady() => tcs.TrySetResult();

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// ожидание разрешения на запуск
		await WaitAsync(stoppingToken);

		// первичная загрузка сразу
		await SafeRefreshAsync(stoppingToken);

		// таймер на постоянное обновление
		using var timer = new PeriodicTimer(interval);

		try
		{
			// периодические обновления
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				await SafeRefreshAsync(stoppingToken);
			}
		}
		catch (OperationCanceledException) { /* нормальное завершение */ }
	}

	private Task WaitAsync(CancellationToken ct = default) => tcs.Task.WaitAsync(ct);

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
			var energoIdRepository = scope.ServiceProvider.GetRequiredService<IEnergoIdRepository>();

			var data = await energoIdRepository.GetAsync(ct);

			SetState(data);

			logger.LogDebug("Выполнено обновление EnergoId");
		}
		catch (OperationCanceledException) { }
		catch (Exception ex)
		{
			logger.LogError(ex, "Сбой обновления EnergoId");
		}
		finally
		{
			_refreshGate.Release();
		}
	}

	private void SetState(IEnumerable<Domain.Entities.EnergoId> data)
	{
		var newState = new EnergoIdState(data);
		Volatile.Write(ref _state, newState);
	}

	#endregion Периодическое обновление

	#region Состояние

	private EnergoIdState _state = EnergoIdState.Empty;

	/// <summary>
	/// Текущее состояние
	/// </summary>
	public IEnergoIdCacheState State => Volatile.Read(ref _state);

	/// <summary>
	/// Обновление вне очереди по требованию
	/// </summary>
	public Task UpdateAsync(CancellationToken ct = default) => SafeRefreshAsync(ct);

	#endregion Состояние
}
