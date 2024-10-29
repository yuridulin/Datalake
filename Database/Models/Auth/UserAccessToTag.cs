namespace Datalake.Database.Models.Auth;

/// <summary>
/// Информация о доступе к тегу
/// </summary>
public class UserAccessToTag : UserAccessToObject
{
	/// <summary>
	/// Глобальный идентификатор тега
	/// </summary>
	public Guid Guid { get; set; }
}
