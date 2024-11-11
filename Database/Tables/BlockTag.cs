using Datalake.Database.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице связей блоков и тегов
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class BlockTag
{
	const string TableName = "BlockTags";

	// поля в БД

	/// <summary>
	/// Идентификатор блока
	/// </summary>
	[Column]
	public int BlockId { get; set; }

	/// <summary>
	/// Идентификатор тега
	/// </summary>
	[Column]
	public int? TagId { get; set; }

	/// <summary>
	/// Название в рамках блока
	/// </summary>
	[Column]
	public string? Name { get; set; } = string.Empty;

	/// <summary>
	/// Тип связи тега к блоку
	/// </summary>
	[Column]
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
