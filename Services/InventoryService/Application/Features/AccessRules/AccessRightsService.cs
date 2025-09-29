using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.PrivateApi.Attributes;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.AccessRights;

namespace Datalake.InventoryService.Application.Features.AccessRules;

/// <summary>
/// Сервис для работы пользователей с правами доступа
/// </summary>
[Scoped]
public class AccessRightsService(
	AccessRightsRepository accessRightsRepository,
	InventoryCacheStore dataStore,
	ILogger<AccessRightsService> logger)
{
	/// <summary>
	/// Применение изменений прав доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="request">Новые права доступа</param>
	public async Task<bool> ApplyChangesAsync(UserAccessEntity user, AccessRightsApplyRequest request)
	{

		DatabaseResult<AccessRuleEntity> result;

		// операции с БД
		if (request.UserGroupGuid.HasValue)
			result = await accessRightsRepository.ReplaceUserGroupRightsAsync(
				request.UserGroupGuid.Value,
				request.Rights.Select(x => new AccessRuleEntity(null, request.UserGroupGuid, false, x.TagId, x.SourceId, x.BlockId, x.AccessType)));

		else if (request.UserGuid.HasValue)
			result = await accessRightsRepository.ReplaceUserRightsAsync(
				request.UserGuid.Value,
				request.Rights.Select(x => new AccessRuleEntity(request.UserGuid, null, false, x.TagId, x.SourceId, x.BlockId, x.AccessType)));

		else if (request.SourceId.HasValue)
			result = await accessRightsRepository.ReplaceSourceRightsAsync(
				request.SourceId.Value,
				request.Rights.Select(x => new AccessRuleEntity(request.UserGuid, request.UserGroupGuid, false, null, request.SourceId, null, x.AccessType)));

		else if (request.BlockId.HasValue);

		else if (request.TagId.HasValue)
			result = await accessRightsRepository.ReplaceTagRightsAsync(
				request.TagId.Value,
				request.Rights.Select(x => new AccessRuleEntity(request.UserGuid, request.UserGroupGuid, false, request.TagId, null, null, x.AccessType)));

		else
			throw new InvalidOperationException("Не прислано достаточно данных для выполнения изменений. Должен быть передан хотя бы один идентификатор");

		// логика изменения кэша
		// если при обновлении кэша мы получим ошибку, стор пересоздаст его на основе актуальной БД
		using (await dataStore.AcquireWriteLockAsync())
		{
			dataStore.UpdateStateWithinLock(state => state with
			{
				AccessRules = state.AccessRules
					.RemoveAll(x => result.DeletedIdentifiers.Contains(x.Id))
					.AddRange(result.AddedEntities)
			});
		}

		// сообщаем, что всё выполнено успешно
		logger.LogInformation("Права доступа изменены");
		return true;
	}
}
