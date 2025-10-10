using Datalake.Domain.Exceptions;
using Datalake.Domain.Interfaces;
using Datalake.Domain.ValueObjects;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице блоков
/// </summary>
public record class Block : IWithIdentityKey, ISoftDeletable
{
	private Block() { }

	/// <summary>
	/// Создание пустого нового блока
	/// </summary>
	/// <param name="parentId">Идентификатор родительского блока</param>
	public Block(int? parentId)
	{
		GlobalId = Guid.NewGuid();
		ParentId = parentId;
	}

	/// <summary>
	/// Создание нового блока на основе переданной базовой информации
	/// </summary>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <param name="name">Имя блока</param>
	/// <param name="description">Описание блока</param>
	public Block(int? parentId, string name, string? description) : this(parentId)
	{
		Name = name;
		Description = description;
	}

	public void UpdateName(string newName)
	{
		Name = newName;
	}

	public void UpdateDescription(string? newDescription)
	{
		Description = newDescription;
	}

	public void UpdateParent(int? newParentId)
	{
		if (newParentId.HasValue && newParentId.Value == Id)
			throw new DomainException("Нельзя переместить блок сам в себя");

		ParentId = newParentId;
	}

	public void MarkAsDeleted()
	{
		if (IsDeleted)
			throw new DomainException("Блок уже удален");

		IsDeleted = true;
	}

	// поля в БД

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

	// связанные данные

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

	/*/// <summary>
	/// Список связанных тегов
	/// </summary>
	public ICollection<Tag> Tags { get; set; } = [];*/

	/// <summary>
	/// Список прямых правил доступа на этот блок
	/// </summary>
	public ICollection<AccessRule> AccessRules { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита по этому блоку
	/// </summary>
	public ICollection<Log> AuditLogs { get; set; } = [];

	/// <summary>
	/// Рассчитаные для этого блока указания фактического доступа
	/// </summary>
	public ICollection<CalculatedAccessRule> CalculatedAccessRules { get; set; }
}
