using Datalake.Database.Interfaces;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице связей тега с другими тегами для вычисления значений
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public record class TagInput : IReadOnlyTagInput
{
	const string TableName = "TagInputs";

	// поля в БД

	/// <inheritdoc/>
	[Column, Key, Identity]
	public int Id { get; set; }

	/// <inheritdoc/>
	[Column]
	public int TagId { get; set; }

	/// <inheritdoc/>
	[Column]
	public int? InputTagId { get; set; }

	/// <inheritdoc/>
	[Column]
	public string VariableName { get; set; } = string.Empty;

	// связи

	/// <summary>
	/// Результирующий тег
	/// </summary>
	public Tag Tag { get; set; } = null!;

	/// <summary>
	/// Входной тег
	/// </summary>
	public Tag? InputTag { get; set; }
}
