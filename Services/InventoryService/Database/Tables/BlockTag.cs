using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Database.Tables;

/// <summary>
/// Запись в таблице связей блоков и тегов
/// </summary>
public record class BlockTag
{
	private BlockTag() { }

	public BlockTag(int blockId, int tagId, string? name, BlockTagRelation relation = BlockTagRelation.Static)
	{
		BlockId = blockId;
		TagId = tagId;
		Name = name;
		Relation = relation;
	}

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
	/// Идентификатор тега
	/// </summary>
	public int? TagId { get; set; }

	/// <summary>
	/// Название в рамках блока
	/// </summary>
	public string? Name { get; set; } = string.Empty;

	/// <summary>
	/// Тип связи тега к блоку
	/// </summary>
	public BlockTagRelation Relation { get; set; } = BlockTagRelation.Static;

	// связи

	/// <summary>
	/// Блок
	/// </summary>
	public Block Block { get; set; } = null!;

	/// <summary>
	/// Тег
	/// </summary>
	public Tag? Tag { get; set; }
}
