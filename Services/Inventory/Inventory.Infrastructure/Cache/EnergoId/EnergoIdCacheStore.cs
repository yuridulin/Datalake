using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Datalake.Inventory.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace Datalake.Inventory.Infrastructure.Cache.EnergoId;

///<summary>
/// Хранилище данные о пользователях EnergoId, регулярно обновляемое.
/// Это inMemory, потому что представление большое и долго грузится
///</summary>
public sealed class EnergoIdCacheStore(
	IServiceScopeFactory scopeFactory,
	ILogger<EnergoIdCacheStore> logger) : BackgroundService, IEnergoIdCache
{
	/// <summary>
	/// Текущее состояние
	/// </summary>
	public IEnergoIdCacheState State => Volatile.Read(ref _state);

	public override async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = scopeFactory.CreateScope();
		var energoIdViewCreator = scope.ServiceProvider.GetRequiredService<IEnergoIdViewCreator>();
		var energoIdRepository = scope.ServiceProvider.GetRequiredService<IEnergoIdRepository>();

		await energoIdViewCreator.RecreateAsync(cancellationToken);

		var data = await energoIdRepository.GetAsync(cancellationToken);

		SetState(data);

		await base.StartAsync(cancellationToken);
	}

	/// <summary>
	/// Публичный ручной триггер
	/// </summary>
	public Task UpdateAsync(CancellationToken ct = default) => SafeRefreshAsync(ct);


	private readonly SemaphoreSlim _refreshGate = new(1, 1);
	static readonly TimeSpan interval = TimeSpan.FromMinutes(1);
	private EnergoIdState _state = new() { Users = [], UsersByGuid = ImmutableDictionary<Guid, EnergoIdEntity>.Empty };

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
			var energoIdRepository = scope.ServiceProvider.GetRequiredService<IEnergoIdRepository>();

			var data = await energoIdRepository.GetAsync(ct);

			SetState(data);

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

	private void SetState(IEnumerable<EnergoIdEntity> data)
	{
		var list = data.ToImmutableList();
		var dict = list.ToImmutableDictionary(x => x.Guid);

		var newState = new EnergoIdState { Users = list, UsersByGuid = dict, };

		Volatile.Write(ref _state, newState);
	}
}
