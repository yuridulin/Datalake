using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.UserGroups;

/// <summary>
/// Информация о группе пользователей
/// </summary>
public class UserGroupInfo : UserGroupSimpleInfo
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
}
