using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;
using Datalake.Domain.ValueObjects;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице групп пользователей
/// </summary>
public record class UserGroup : IWithGuidKey, ISoftDeletable
{
	#region Конструкторы

	private UserGroup() { }

	/// <summary>
	/// Создание новой группы учетных записей
	/// </summary>
	/// <param name="parentGuid">Идентификатор родительской группы</param>
	/// <param name="name">Название</param>
	/// <param name="description">Описание</param>
	public static UserGroup Create(Guid? parentGuid, string? name, string? description)
	{
		var group = new UserGroup
		{
			Guid = Guid.NewGuid(),
		};

		group.UpdateParent(parentGuid);
		group.Update(name, description);

		return group;
	}

	#endregion Конструкторы

	#region Методы

	/// <summary>
	/// Изменение родительской группы, в которую входит эта группа
	/// </summary>
	/// <param name="parentGuid">Идентификатор родительской группы</param>
	/// <exception cref="DomainException">Действие не разрешено</exception>
	public void UpdateParent(Guid? parentGuid)
	{
		if (parentGuid.HasValue && parentGuid.Value == Guid)
			throw new DomainException("Группа учетных записей не может быть вложена в саму себя");

		ParentGuid = parentGuid;
	}

	/// <summary>
	/// Изменение свойств группы
	/// </summary>
	/// <param name="name">Название</param>
	/// <param name="description">Описание</param>
	/// <exception cref="DomainException">Значения не корректны</exception>
	public void Update(string? name, string? description)
	{
		if (string.IsNullOrEmpty(name))
			throw new DomainException("Имя группы учетных записей является обязательным");

		Name = name;
		Description = description;
	}

	/// <inheritdoc/>
	public void MarkAsDeleted()
	{
		if (IsDeleted)
			throw new DomainException("Группа учетных записей уже удалена");

		IsDeleted = true;
	}

	#endregion Методы

	#region Свойства

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

	#endregion Свойства

	#region Связи

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

	#endregion Связи
}
