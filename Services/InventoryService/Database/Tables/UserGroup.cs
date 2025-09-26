namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице групп пользователей
/// </summary>
public record class UserGroup
{
	private UserGroup() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Guid { get; set; }

	/// <summary>
	/// Идентификатор родительской группы
	/// </summary>
	public Guid? ParentGuid { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Группа отмечена как удаленная
	/// </summary>
	public bool IsDeleted { get; set; } = false;

	// связи

	/// <summary>
	/// Родительская группа
	/// </summary>
	public UserGroup? Parent { get; set; }

	/// <summary>
	/// Список подгрупп
	/// </summary>
	public ICollection<UserGroup> Children { get; set; } = [];

	/// <summary>
	/// Список связей с пользователями
	/// </summary>
	public ICollection<UserGroupRelation> UsersRelations { get; set; } = [];

	/// <summary>
	/// Список пользователей
	/// </summary>
	public ICollection<User> Users { get; set; } = [];

	/// <summary>
	/// Список правил доступа, выданных этой группе
	/// </summary>
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<Log> Logs { get; set; } = null!;
}
