using Datalake.Database.Interfaces;
using Datalake.PublicApi.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице связей блоков и тегов
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public record class BlockTag : IReadOnlyBlockTag
{
	const string TableName = "BlockTags";

	// поля в БД

	/// <inheritdoc/>
	[Column]
	public int BlockId { get; set; }

	/// <inheritdoc/>
	[Column]
	public int? TagId { get; set; }

	/// <inheritdoc/>
	[Column]
	public string? Name { get; set; } = string.Empty;

	/// <inheritdoc/>
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
