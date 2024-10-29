namespace Datalake.Database.Models.Auth;

/// <summary>
/// Информация о доступе к группе пользователей
/// </summary>
public class UserAccessToGroup
{
	/// <summary>
	/// Идентификатор группы
	/// </summary>
	public Guid Guid { get; set; }

	/// <summary>
	/// Правило доступа
	/// </summary>
	public AccessRule Rule { get; set; } = null!;
}
