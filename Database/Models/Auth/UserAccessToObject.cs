namespace Datalake.Database.Models.Auth;

/// <summary>
/// Информация о доступе к объекту
/// </summary>
public class UserAccessToObject
{
	/// <summary>
	/// Идентификатор объекта
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Правило доступа
	/// </summary>
	public AccessRule Rule { get; set; } = null!;
}
