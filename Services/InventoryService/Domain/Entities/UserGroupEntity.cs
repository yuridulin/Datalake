using Datalake.InventoryService.Domain.Interfaces;

namespace Datalake.InventoryService.Domain.Entities;

/// <summary>
/// Запись в таблице групп пользователей
/// </summary>
public record class UserGroupEntity : IWithGuidKey, ISoftDeletable
{
	private UserGroupEntity() { }

	public void MarkAsDeleted()
	{
		IsDeleted = true;
	}

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public Guid Guid { get; private set; }

	/// <summary>
	/// Идентификатор родительской группы
	/// </summary>
	public Guid? ParentGuid { get; private set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; private set; } = string.Empty;

	/// <summary>
	/// Описание
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// Группа отмечена как удаленная
	/// </summary>
	public bool IsDeleted { get; private set; } = false;

	// связи

	/// <summary>
	/// Родительская группа
	/// </summary>
	public UserGroupEntity? Parent { get; set; }

	/// <summary>
	/// Список подгрупп
	/// </summary>
	public ICollection<UserGroupEntity> Children { get; set; } = [];

	/// <summary>
	/// Список связей с пользователями
	/// </summary>
	public ICollection<UserGroupRelationEntity> UsersRelations { get; set; } = [];

	/// <summary>
	/// Список пользователей
	/// </summary>
	public ICollection<UserEntity> Users { get; set; } = [];

	/// <summary>
	/// Список правил доступа, выданных этой группе
	/// </summary>
	public ICollection<AccessRuleEntity> AccessRightsList { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<AuditEntity> Logs { get; set; } = null!;
}
