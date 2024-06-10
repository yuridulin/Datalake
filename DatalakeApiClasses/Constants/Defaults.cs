using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Models.Users;

namespace DatalakeApiClasses.Constants;

/// <summary>
/// Данные, которые используются при инициализации приложения в новом окружении
/// </summary>
public static class Defaults
{
	/// <summary>
	/// Учётная запись по умолчанию
	/// </summary>
	public static readonly UserCreateRequest InitialAdmin = new()
	{
		Login = "admin",
		Password = "admin",
		AccessType = AccessType.Admin,
		FullName = "Администратор",
	};
}
