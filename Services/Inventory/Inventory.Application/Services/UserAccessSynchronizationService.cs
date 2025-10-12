using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Services;

[Singleton]
public class UserAccessSynchronizationService(
	IInventoryCache inventoryCache,
	IUserAccessCalculationService userAccessCalculationService,
	IUserAccessCache userAccessCache,
	IServiceScopeFactory serviceScopeFactory,
	ILogger<UserAccessSynchronizationService> logger) : IUserAccessSynchronizationService
{
	private IUserAccessCacheState? previousUsersAccessState;
	private bool started = false;

	public void Start()
	{
		if (started)
			return;

		inventoryCache.StateChanged += (_, newState) =>
		{
			_ = Task.Run(CalculateAndUpdateUsersAccess);
		};

		userAccessCache.StateChanged += (_, newState) =>
		{
			_ = Task.Run(CompareAndSaveUsersAccess);
		};

		started = true;

		logger.LogInformation("Синхронизация кэша прав доступа настроена");
	}

	/// <summary>
	/// Вычисление прав доступа после изменения структуры объектов и запись в кэш прав доступа
	/// </summary>
	private async Task CalculateAndUpdateUsersAccess()
	{
		try
		{
			var inventoryState = inventoryCache.State;
			var usersAccess = userAccessCalculationService.CalculateAccess(inventoryState);

			await userAccessCache.SetAsync(usersAccess);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка при расчете прав доступа после изменений в структуре");
		}
	}

	/// <summary>
	/// Обработка вычисленных прав доступа после обновления кэша
	/// Вычисление изменений, сохранение их в хранилище
	/// </summary>
	private async Task CompareAndSaveUsersAccess()
	{
		List<CalculatedAccessRule> updatedRules = [];

		try
		{
			// сравнение текущего и последнего сохраненного состояний
			// сделать семафор, чтобы избежать гонок?
			var oldState = previousUsersAccessState;
			var newState = userAccessCache.State;
			Interlocked.Exchange(ref previousUsersAccessState, newState);

			// TODO: вычисление изменений между состояниями (основная работа)

			// TODO: сериализация в CalculatedAccessRule[]
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка при вычислении изменений в правах доступа");
		}

		if (updatedRules.Count == 0)
			return;

		// сохранение
		_ = SaveUpdatedRules(updatedRules);
	}

	/// <summary>
	/// Сохранение изменившихся вычисленных прав доступа
	/// </summary>
	/// <param name="updatedRules">Список изменений</param>
	private async Task SaveUpdatedRules(List<CalculatedAccessRule> updatedRules)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var repository = scope.ServiceProvider.GetRequiredService<ICalculatedAccessRulesRepository>();

		await repository.UpdateAsync(updatedRules);
	}
}
