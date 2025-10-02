using Datalake.Contracts.Public.Enums;

namespace Datalake.PublicApi.Contracts;

/// <summary>
/// Информация для создания определенной статичной учетной записи
/// </summary>
public record class StaticUsersOptionsDto
{
	/// <summary>
	/// Имя учетной записи
	/// </summary>
	public required string Name { get; init; }

	/// <summary>
	/// Токен доступа
	/// </summary>
	public required string Token { get; init; }

	/// <summary>
	/// Уровень доступа
	/// </summary>
	public required AccessType AccessType { get; init; }

	/// <summary>
	/// Адрес, с которого разрешен вход
	/// </summary>
	public string? Host { get; init; }
}
