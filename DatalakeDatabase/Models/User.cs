using DatalakeDatabase.Enums;
using System.ComponentModel.DataAnnotations;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class User
{
	const string TableName = "Users";

	// поля в БД

	[Column, Key]
	public string Name { get; set; } = string.Empty;

	[Column]
	public string Hash { get; set; } = string.Empty;

	[Column]
	public AccessType AccessType { get; set; }

	[Column]
	public string FullName { get; set; } = string.Empty;

	[Column]
	public string? StaticHost { get; set; }
}
