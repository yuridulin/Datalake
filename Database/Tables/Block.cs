using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице блоков
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class Block
{
	const string TableName = "Blocks";

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	[Column]
	public Guid GlobalId { get; set; }

	/// <summary>
	/// Идентификатор родительского блока
	/// </summary>
	[Column]
	public int? ParentId { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	[Column]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Описание
	/// </summary>
	[Column]
	public string? Description { get; set; }

	/// <summary>
	/// Блок отмечен как удаленный
	/// </summary>
	[Column, Required]
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
