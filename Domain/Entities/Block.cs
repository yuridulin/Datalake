using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;
using Datalake.Domain.ValueObjects;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице блоков
/// </summary>
public record class Block : IWithIdentityKey, ISoftDeletable
{
	#region Конструкторы

	private Block() { }

	/// <summary>
	/// Создание пустого нового блока
	/// </summary>
	/// <param name="parentId">Идентификатор родительского блока</param>
	public static Block CreateEmpty(int? parentId)
	{
		var block = new Block
		{
			GlobalId = Guid.NewGuid(),
			ParentId = parentId,
			Name = string.Empty,
		};

		return block;
	}

	/// <summary>
	/// Создание нового блока на основе переданной базовой информации
	/// </summary>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <param name="name">Имя блока</param>
	/// <param name="description">Описание блока</param>
	public static Block Create(int? parentId, string name, string? description)
	{
		var block = CreateEmpty(parentId);

		block.Name = name;
		block.Description = description;

		return block;
	}

	#endregion Конструкторы

	#region Методы

	/// <summary>
	/// Изменение имени
	/// </summary>
	/// <param name="newName">Новое имя</param>
	public void UpdateName(string newName)
	{
		Name = newName;
	}

	/// <summary>
	/// Изменение описания
	/// </summary>
	/// <param name="newDescription">Новое описание</param>
	public void UpdateDescription(string? newDescription)
	{
		Description = newDescription;
	}

	/// <summary>
	/// Изменение родительского блока
	/// </summary>
	/// <param name="newParentId">Идентификатор родительского блока</param>
	/// <exception cref="DomainException">Блок родитель сам себе</exception>
	public void UpdateParent(int? newParentId)
	{
		if (newParentId.HasValue && newParentId.Value == Id)
			throw new DomainException("Нельзя переместить блок сам в себя");

		ParentId = newParentId;
	}

	/// <inheritdoc />
	public void MarkAsDeleted()
	{
		if (IsDeleted)
			throw new DomainException("Блок уже удален");

		IsDeleted = true;
	}

	#endregion Методы

	#region Свойства

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; private set; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	public Guid GlobalId { get; private set; }

	/// <summary>
	/// Идентификатор родительского блока
	/// </summary>
	public int? ParentId { get; private set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; private set; } = string.Empty;

	/// <summary>
	/// Описание
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// Блок отмечен как удаленный
	/// </summary>
	public bool IsDeleted { get; private set; } = false;

	#endregion Свойства

	#region Связи

	/// <summary>
	/// Родительский блок
	/// </summary>
	public Block? Parent { get; set; }

	/// <summary>
	/// Список вложенных блоков
	/// </summary>
	public ICollection<Block> Children { get; set; } = [];

	/// <summary>
	/// Список свойств
	/// </summary>
	public ICollection<BlockProperty> Properties { get; set; } = [];

	/// <summary>
	/// Список связей с тегами
	/// </summary>
	public ICollection<BlockTag> RelationsToTags { get; set; } = [];

	/// <summary>
	/// Список прямых правил доступа на этот блок
	/// </summary>
	public ICollection<AccessRule> AccessRules { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита по этому блоку
	/// </summary>
	public ICollection<AuditLog> AuditLogs { get; set; } = [];

	/// <summary>
	/// Рассчитаные для этого блока указания фактического доступа
	/// </summary>
	public ICollection<CalculatedAccessRule> CalculatedAccessRules { get; set; } = [];

	#endregion Связи
}
