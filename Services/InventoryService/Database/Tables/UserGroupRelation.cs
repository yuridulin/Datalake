using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице связей учетных записей и групп пользователей
/// </summary>
public record class UserGroupRelation
{
	private UserGroupRelation() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Идентификатор учетной записи
	/// </summary>
	public required Guid UserGuid { get; set; }

	/// <summary>
	/// Идентификатор группы пользователей
	/// </summary>
	public required Guid UserGroupGuid { get; set; }

	/// <summary>
	/// Уровень доступа пользователя к группе
	/// </summary>
	public AccessType AccessType { get; set; }

	// связи

	/// <summary>
	/// Пользователь
	/// </summary>
	public User User { get; set; } = null!;

	/// <summary>
	/// Группа пользователей
	/// </summary>
	public UserGroup UserGroup { get; set; } = null!;
}
