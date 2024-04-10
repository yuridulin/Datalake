namespace DatalakeDatabase.ApiModels.Blocks;

/// <summary>
/// Информация о сущности
/// </summary>
public class BlockSimpleInfo
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; } = 0;

	/// <summary>
	/// Наименование
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Текстовое описание
	/// </summary>
	public string? Description { get; set; }
}
