using System.Text.Json.Serialization;

namespace Datalake.ApiClasses.Enums;

/// <summary>
/// Тип учётной записи
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserType
{
	/// <summary>
	/// Локальная учётная запись, проверяемая через логин/пароль
	/// </summary>
	Local = 1,

	/// <summary>
	/// Учётная запись для общения по API разных модулей
	/// </summary>
	Static = 2,

	/// <summary>
	/// Учётная запись, доступ к которой идёт по проверке на удалённом Keycloak сервере
	/// </summary>
	EnergoId = 3,
}
