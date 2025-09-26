using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице свойств блоков
/// </summary>
public record class BlockProperty
{
	private BlockProperty() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Идентификатор блока
	/// </summary>
	public int BlockId { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Тип значения
	/// </summary>
	public TagType Type { get; set; } = TagType.String;

	/// <summary>
	/// Значение
	/// </summary>
	public string Value { get; set; } = string.Empty;

	// связи

	/// <summary>
	/// Блок
	/// </summary>
	public Block? Block { get; set; }
}
