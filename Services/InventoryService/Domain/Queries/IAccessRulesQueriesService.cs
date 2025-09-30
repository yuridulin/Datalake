﻿using Datalake.PublicApi.Models.AccessRights;

namespace Datalake.InventoryService.Domain.Queries;

/// <summary>
/// Запросы, связанные с правилами прав доступа
/// </summary>
public interface IAccessRulesQueriesService
{
	/// <summary>
	/// Получение списка правил прав доступа
	/// </summary>
	Task<IEnumerable<AccessRightsInfo>> GetAsync(
		Guid? userGuid = null,
		Guid? userGroupGuid = null,
		int? sourceId = null,
		int? blockId = null,
		int? tagId = null,
		CancellationToken ct = default);
}