using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Datalake.Inventory.Application.Services;

/// <summary>
/// Оркестратор обновления зависимых данных
/// </summary>
[Singleton]
public class UserAccessSynchronizationService
{
	private readonly IInventoryCache inventoryCache;
	private readonly IUserAccessCache userAccessCache;
	private readonly IUserAccessCalculationService userAccessCalculationService;
	private readonly IServiceScopeFactory serviceScopeFactory;
	private IUserAccessCacheState? previousUsersAccessState;

	public UserAccessSynchronizationService(
		IInventoryCache inventoryCache,
		IUserAccessCalculationService userAccessCalculationService,
		IUserAccessCache userAccessCache,
		IServiceScopeFactory serviceScopeFactory)
	{
		this.inventoryCache = inventoryCache;
		this.userAccessCalculationService = userAccessCalculationService;
		this.userAccessCache = userAccessCache;
		this.serviceScopeFactory = serviceScopeFactory;

		// сначала нужно восстановить состояния, чтобы избежать ненужных обновлений того же самого
		// это нам уже должен был сделать сервис запуска
		// и теперь подписываемся на изменения

		inventoryCache.StateChanged += (_, newState) =>
		{
			_ = Task.Run(CalculateAndUpdateUsersAccess);
		};

		userAccessCache.StateChanged += (_, newState) =>
		{
			_ = Task.Run(CompareAndSaveUsersAccess);
		};
	}

	/// <summary>
	/// Вычисление прав доступа после изменения структуры объектов и запись в кэш прав доступа
	/// </summary>
	private async Task CalculateAndUpdateUsersAccess()
	{
		var inventoryState = inventoryCache.State;
		var usersAccess = userAccessCalculationService.CalculateAccess(inventoryState);

		await userAccessCache.SetAsync(usersAccess);
	}

	/// <summary>
	/// Обработка вычисленных прав доступа после обновления кэша
	/// Вычисление изменений, сохранение их в хранилище
	/// </summary>
	private async Task CompareAndSaveUsersAccess()
	{
		// сравнение текущего и последнего сохраненного состояний
		// сделать семафор, чтобы избежать гонок?
		var oldState = previousUsersAccessState;
		var newState = userAccessCache.State;
		Interlocked.Exchange(ref previousUsersAccessState, newState);

		// TODO: вычисление изменений между состояниями (основная работа)

		// TODO: сериализация в CalculatedAccessRule[]
		List<CalculatedAccessRule> updatedRules = [];

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
