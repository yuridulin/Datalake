using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Users;

/// <summary>
/// Расширенная информация о пользователе, включающая данные для аутентификации
/// </summary>
public class UserDetailInfo : UserInfo
{
	/// <summary>
	/// Хэш, по которому проверяется доступ
	/// </summary>
	public string? Hash { get; set; }

	/// <summary>
	/// Адрес статического узла, с которого идёт доступ
	/// </summary>
	public string? StaticHost { get; set; }
}
