using Datalake.Database.Enums;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class Source
{
	const string TableName = "Sources";

	// поля в БД

	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Column, NotNull]
	public string Name { get; set; } = string.Empty;

	[Column]
	public string? Description { get; set; }

	[Column]
	public SourceType Type { get; set; } = SourceType.Inopc;

	[Column]
	public string? Address { get; set; }

	// связи

	public ICollection<Tag> Tags { get; set; } = [];

	public ICollection<AccessRights> AccessRightsList { get; set; } = [];
}
