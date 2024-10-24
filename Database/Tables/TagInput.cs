using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class TagInput
{
	public const string TableName = "TagInputs";

	// поля в БД

	[Column, Key, Identity]
	public int Id { get; set; }

	[Column]
	public int TagId { get; set; }

	[Column]
	public int? InputTagId { get; set; }

	[Column]
	public string VariableName { get; set; } = string.Empty;

	// связи

	public Tag Tag { get; set; } = null!;

	public Tag? InputTag { get; set; }
}
