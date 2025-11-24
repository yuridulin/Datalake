using Datalake.Contracts.Interfaces;
using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.UserGroups;

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
	/// Общий уровень доступа для всех участников группы
	/// </summary>
	[Required]
	public required AccessType GlobalAccessType { get; set; }

	/// <summary>
	/// Правило доступа к этой группе
	/// </summary>
	[Required]
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
