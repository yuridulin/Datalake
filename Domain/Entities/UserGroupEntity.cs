using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице групп пользователей
/// </summary>
public record class UserGroupEntity : IWithGuidKey, ISoftDeletable
{
	private UserGroupEntity() { }

	public UserGroupEntity(Guid? parentGuid, string? name, string? description)
	{
		UpdateParent(parentGuid);
		Update(name, description);
	}

	public void UpdateParent(Guid? parentGuid)
	{
		if (parentGuid.HasValue && parentGuid.Value == Guid)
			throw new DomainException("Группа учетных записей не может быть вложена в саму себя");

		ParentGuid = parentGuid;
	}

	public void Update(string? name, string? description)
	{
		if (string.IsNullOrEmpty(name))
			throw new DomainException("Имя группы учетных записей является обязательным");

		Name = name;
		Description = description;
	}

	public void MarkAsDeleted()
	{
		if (IsDeleted)
			throw new DomainException("Группа учетных записей уже удалена");

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
	public ICollection<AccessRuleEntity> AccessRules { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<AuditEntity> Logs { get; set; } = null!;
}
