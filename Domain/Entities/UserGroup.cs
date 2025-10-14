using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;
using Datalake.Domain.ValueObjects;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице групп пользователей
/// </summary>
public record class UserGroup : IWithGuidKey, ISoftDeletable
{
	private UserGroup() { }

	public UserGroup(Guid? parentGuid, string? name, string? description)
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
	/// Список прямых правил доступа, выданных этой группе учетных записей
	/// </summary>
	public ICollection<AccessRule> AccessRules { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита по этой группе учетных записей
	/// </summary>
	public ICollection<AuditLog> AuditLogs { get; set; } = [];

	/// <summary>
	/// Рассчитаные для этой группы учетных записей указания фактического доступа
	/// </summary>
	public ICollection<CalculatedAccessRule> CalculatedAccessRules { get; set; } = [];
}
