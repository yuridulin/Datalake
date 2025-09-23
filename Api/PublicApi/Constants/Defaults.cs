using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Users;

namespace Datalake.PublicApi.Constants;

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
		Type = UserType.Local,
	};

	/// <summary>
	/// Основной путь всех API запросов
	/// </summary>
	public const string ApiRoot = "api";
}
