using Datalake.Database.Abstractions;
using Datalake.Database.Models.Auth;

namespace Datalake.Database.Models.UserGroups;

/// <summary>
/// Информация о группе пользователей
/// </summary>
public class UserGroupInfo : UserGroupSimpleInfo, IProtectedEntity
{
	/// <summary>
	/// Произвольное описание группы
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Идентификатор группы, в которой располагается эта группа
	/// </summary>
	public Guid? ParentGroupGuid { get; set; }

	/// <summary>
	/// Правило доступа
	/// </summary>
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
