using Datalake.Database.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице свойств блоков
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class BlockProperty
{
	const string TableName = "BlockProperties";

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
	/// Название
	/// </summary>
	[Column, NotNull]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Тип значения
	/// </summary>
	[Column]
	public TagType Type { get; set; } = TagType.String;

	/// <summary>
	/// Значение
	/// </summary>
	[Column]
	public string Value { get; set; } = string.Empty;

	// связи

	/// <summary>
	/// Блок
	/// </summary>
	public Block? Block { get; set; }
}
