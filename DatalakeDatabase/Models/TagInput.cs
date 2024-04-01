using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Keyless, Table(TableName), LinqToDB.Mapping.Table(TableName)]
public partial class TagInput
{
	public const string TableName = "TagInputs";

	// поля в БД

	[Column]
	public int TagId { get; set; }

	[Column]
	public int InputTagId { get; set; }

	[Column]
	public string VariableName { get; set; } = string.Empty;

	// связи

	[ForeignKey(nameof(TagId))]
	public Tag? Tag { get; set; }

	[NotMapped]
	public Tag? InputTag { get; set; }
}
