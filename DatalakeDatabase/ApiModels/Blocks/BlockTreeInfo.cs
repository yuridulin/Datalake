namespace DatalakeDatabase.ApiModels.Blocks;

/// <summary>
/// Информация о сущности в иерархическом представлении
/// </summary>
public class BlockTreeInfo
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Наименование
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Текстовое описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Вложенные сущности, подчинённые этой
	/// </summary>
	public BlockTreeInfo[] Children { get; set; } = [];
}
