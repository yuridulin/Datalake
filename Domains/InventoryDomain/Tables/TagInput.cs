using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице связей тега с другими тегами для вычисления значений
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public record class TagInput
{
	/// <summary>
	/// Название таблицы
	/// </summary>
	public const string TableName = "TagInputs";

	/// <summary>Конструктор для LinqToDB</summary>
	public TagInput() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Column, Key, Identity]
	public int Id { get; set; }

	/// <summary>
	/// Идентификатор результиющего тега
	/// </summary>
	[Column]
	public int TagId { get; set; }

	/// <summary>
	/// Идентификатор входного тега
	/// </summary>
	[Column]
	public int? InputTagId { get; set; }

	/// <summary>
	/// Идентификатор блока с входным тегом
	/// </summary>
	[Column]
	public int? InputBlockId { get; set; }

	/// <summary>
	/// Имя переменной в формуле
	/// </summary>
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
