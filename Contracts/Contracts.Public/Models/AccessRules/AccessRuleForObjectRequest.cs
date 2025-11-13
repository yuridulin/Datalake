using Datalake.Contracts.Public.Enums;

namespace Datalake.Contracts.Public.Models.AccessRules;

/// <summary>
/// Необходимая информация для создания правила доступа для кокретного действующего лица
/// </summary>
/// <param name="AccessType">Тип доступа</param>
/// <param name="UserGuid">Идентификатор учетной записи</param>
/// <param name="UserGroupGuid">Идентификатор группы учетных записей</param>
public record AccessRuleForObjectRequest(
	AccessType AccessType,
	Guid? UserGuid = null,
	Guid? UserGroupGuid = null);
