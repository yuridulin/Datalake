namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице блоков
/// </summary>
public record class Block
{
	private Block() { }

	/// <summary>
	/// Создание пустого нового блока
	/// </summary>
	/// <param name="guid">Глобальный идентификатор</param>
	/// <param name="parentId">Идентификатор родительского блока</param>
	public Block(Guid guid, int? parentId)
	{
		GlobalId = guid;
		ParentId = parentId;
	}

	/// <summary>
	/// Создание нового блока на основе переданной базовой информации
	/// </summary>
	/// <param name="guid">Глобальный идентификатор</param>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <param name="name">Имя блока</param>
	/// <param name="description">Описание блока</param>
	public Block(Guid guid, int? parentId, string name, string? description)
	{
		GlobalId = guid;
		ParentId = parentId;
		Name = name;
		Description = description;
	}

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	public Guid GlobalId { get; set; }

	/// <summary>
	/// Идентификатор родительского блока
	/// </summary>
	public int? ParentId { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Блок отмечен как удаленный
	/// </summary>
	public bool IsDeleted { get; set; } = false;

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

	/// <summary>
	/// Список связанных тегов
	/// </summary>
	public ICollection<Tag> Tags { get; set; } = [];

	/// <summary>
	/// Список правил доступа, выданных на этот блок
	/// </summary>
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];

	/// <summary>
	/// Список сообщений аудита
	/// </summary>
	public ICollection<Log> Logs { get; set; } = null!;
}
