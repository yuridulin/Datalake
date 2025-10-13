using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Features.CalculatedAccessRules.Commands.UpdateRules;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Entities;
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
		logger.LogInformation("Настраивается синхронизация кэша прав доступа...");

		if (started)
		{
			logger.LogWarning("Cинхронизация кэша прав доступа уже была настроена, выполнение настройки прекращается");
			return;
		}

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
		logger.LogInformation("Выполняется перерасчет прав доступа");

		try
		{
			var newInventoryState = inventoryCache.State;
			var usersAccess = userAccessCalculationService.CalculateAccess(newInventoryState);
			logger.LogInformation("Перерасчет прав доступа выполнен");

			await userAccessCache.SetAsync(usersAccess);
			logger.LogInformation("Рассчитанные права доступа записаны в кэш");
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
		logger.LogInformation("Выполняется вычисление изменений в рассчитанных правах доступа");
		List<CalculatedAccessRule> updatedRules = [];

		try
		{
			// сравнение текущего и последнего сохраненного состояний
			// сделать семафор, чтобы избежать гонок?
			var oldState = previousUsersAccessState;
			var newState = userAccessCache.State;

			// вычисление изменений между состояниями прав
			List<CalculatedAccessRule> rulesToAddOrUpdate = [];
			var inventoryState = inventoryCache.State;

			if (oldState == null)
			{
				// все правила - на добавление
				foreach (var userEntity in newState.UsersAccess.Values)
				{
					rulesToAddOrUpdate.AddRange(GetAllRules(inventoryState, userEntity));
				}
			}
			else
			{
				// по каждому существующему пользователю проверяем, что и как
				foreach (var userGuid in inventoryState.Users.Keys)
				{
					// пользователь добавлен, а прав нет - невалидное состояние, не обрабатываем
					if (!newState.UsersAccess.TryGetValue(userGuid, out var newEntity))
						continue;

					// по пользователю нет прав в прошлой версии - все на добавление
					if (!oldState.UsersAccess.TryGetValue(userGuid, out var oldEntity))
						rulesToAddOrUpdate.AddRange(GetAllRules(inventoryState, newEntity));

					// проверяем, отличаются ли состояния прав по значению
					if (newEntity != oldEntity)
						rulesToAddOrUpdate.AddRange(GetDiffRules(inventoryState, oldEntity, newEntity));
				}
			}

			Interlocked.Exchange(ref previousUsersAccessState, newState);
			logger.LogInformation("Вычисление изменений в рассчитанных правах доступа завершено. Изменений: {count}", rulesToAddOrUpdate.Count);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка при вычислении изменений в рассчитанных правах доступа");
		}

		// сохранение
		await SaveUpdatedRules(updatedRules);
	}

	/// <summary>
	/// Сохранение изменившихся вычисленных прав доступа
	/// </summary>
	/// <param name="updatedRules">Список изменений</param>
	private async Task SaveUpdatedRules(List<CalculatedAccessRule> updatedRules)
	{
		if (updatedRules.Count == 0)
			return;

		logger.LogInformation("Выполняется сохранение изменений в рассчитанных правах доступа");

		try
		{
			await using var scope = serviceScopeFactory.CreateAsyncScope();
			var handler = scope.ServiceProvider.GetRequiredService<IUpdateCalculatedAccessRulesHandler>();
			await handler.HandleAsync(new() { Rules = updatedRules });

			logger.LogInformation("Изменения рассчитанных прав доступа сохранены");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка при сохранении изменений в рассчитанных правах доступа");
		}
	}

	private static List<CalculatedAccessRule> GetDiffRules(IInventoryCacheState state, UserAccessEntity previous, UserAccessEntity next)
	{
		List<CalculatedAccessRule> diff = [];

		/* Мы сравниваем правила напрямую, т.к. это записи */

		if (previous.RootRule != next.RootRule)
			diff.Add(CalculatedAccessRule.Global(next.Guid, next.RootRule.Access, next.RootRule.Id));

		foreach (var blockId in state.Blocks.Keys)
		{
			if (!next.BlocksRules.TryGetValue(blockId, out var nextRule))
				continue;

			if (!previous.BlocksRules.TryGetValue(blockId, out var previousRule))
				diff.Add(CalculatedAccessRule.ForBlock(next.Guid, blockId, nextRule.Access, nextRule.Id));

			if (previousRule != nextRule)
				diff.Add(CalculatedAccessRule.ForBlock(next.Guid, blockId, nextRule.Access, nextRule.Id));
		}

		foreach (var sourceId in state.Sources.Keys)
		{
			if (!next.SourcesRules.TryGetValue(sourceId, out var nextRule))
				continue;

			if (!previous.SourcesRules.TryGetValue(sourceId, out var previousRule))
				diff.Add(CalculatedAccessRule.ForSource(next.Guid, sourceId, nextRule.Access, nextRule.Id));

			if (previousRule != nextRule)
				diff.Add(CalculatedAccessRule.ForSource(next.Guid, sourceId, nextRule.Access, nextRule.Id));
		}

		foreach (var tagId in state.Tags.Keys)
		{
			if (!next.TagsRules.TryGetValue(tagId, out var nextRule))
				continue;

			if (!previous.TagsRules.TryGetValue(tagId, out var previousRule))
				diff.Add(CalculatedAccessRule.ForTag(next.Guid, tagId, nextRule.Access, nextRule.Id));

			if (previousRule != nextRule)
				diff.Add(CalculatedAccessRule.ForTag(next.Guid, tagId, nextRule.Access, nextRule.Id));
		}

		foreach (var userGroupGuid in state.UserGroups.Keys)
		{
			if (!next.GroupsRules.TryGetValue(userGroupGuid, out var nextRule))
				continue;

			if (!previous.GroupsRules.TryGetValue(userGroupGuid, out var previousRule))
				diff.Add(CalculatedAccessRule.ForUserGroup(next.Guid, userGroupGuid, nextRule.Access, nextRule.Id));

			if (previousRule != nextRule)
				diff.Add(CalculatedAccessRule.ForUserGroup(next.Guid, userGroupGuid, nextRule.Access, nextRule.Id));
		}

		return diff;
	}

	private static List<CalculatedAccessRule> GetAllRules(IInventoryCacheState state, UserAccessEntity next)
	{
		List<CalculatedAccessRule> rules = [];
		rules.Add(CalculatedAccessRule.Global(next.Guid, next.RootRule.Access, next.RootRule.Id));

		foreach (var blockId in state.Blocks.Keys)
		{
			if (!next.BlocksRules.TryGetValue(blockId, out var nextRule))
				continue;

			rules.Add(CalculatedAccessRule.ForBlock(next.Guid, blockId, nextRule.Access, nextRule.Id));
		}

		foreach (var sourceId in state.Sources.Keys)
		{
			if (!next.SourcesRules.TryGetValue(sourceId, out var nextRule))
				continue;

			rules.Add(CalculatedAccessRule.ForSource(next.Guid, sourceId, nextRule.Access, nextRule.Id));
		}

		foreach (var tagId in state.Tags.Keys)
		{
			if (!next.TagsRules.TryGetValue(tagId, out var nextRule))
				continue;

			rules.Add(CalculatedAccessRule.ForTag(next.Guid, tagId, nextRule.Access, nextRule.Id));
		}

		foreach (var userGroupGuid in state.UserGroups.Keys)
		{
			if (!next.GroupsRules.TryGetValue(userGroupGuid, out var nextRule))
				continue;

			rules.Add(CalculatedAccessRule.ForUserGroup(next.Guid, userGroupGuid, nextRule.Access, nextRule.Id));
		}

		return rules;
	}
}
