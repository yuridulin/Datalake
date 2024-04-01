using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public partial class User
{
	const string TableName = "Users";

	// поля в БД

	[Column, Key]
	public string Name { get; set; } = null!;

	[Column]
	public string Hash { get; set; } = null!;

	[Column]
	public AccessType AccessType { get; set; }

	[Column]
	public string FullName { get; set; } = null!;

	[Column]
	public string? StaticHost { get; set; }
}
