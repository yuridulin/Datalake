using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Interfaces;

namespace Datalake.Domain.Entities;

/// <summary>
/// Запись в таблице связей блоков и тегов
/// </summary>
public record class BlockTag : IWithIdentityKey
{
	private BlockTag() { }

	/// <summary>
	/// Создание новой связи блока с тегом
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="name">Локальное имя тега</param>
	/// <param name="relation">Тип связи</param>
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
	public int Id { get; private set; }

	/// <summary>
	/// Идентификатор блока
	/// </summary>
	public int BlockId { get; private set; }

	/// <summary>
	/// Идентификатор тега
	/// </summary>
	public int? TagId { get; private set; }

	/// <summary>
	/// Название в рамках блока
	/// </summary>
	public string? Name { get; private set; } = string.Empty;

	/// <summary>
	/// Тип связи тега к блоку
	/// </summary>
	public BlockTagRelation Relation { get; private set; } = BlockTagRelation.Static;

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
