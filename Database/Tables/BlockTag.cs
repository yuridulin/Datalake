using Datalake.PublicApi.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице связей блоков и тегов
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public record class BlockTag
{
	const string TableName = "BlockTags";

	/// <summary>Конструктор для LinqToDB</summary>
	public BlockTag() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

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
