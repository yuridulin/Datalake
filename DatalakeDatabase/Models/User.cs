using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class User
{
	const string TableName = "Users";

	// поля в БД

	[Column, Key, NotNull]
	public required Guid UserGuid { get; set; }

	[Column, NotNull]
	public required string Name { get; set; }

	[Column, NotNull]
	public required string Hash { get; set; }

	[Column, NotNull]
	public required AccessType AccessType { get; set; }

	[Column]
	public string? FullName { get; set; } = string.Empty;

	[Column]
	public string? StaticHost { get; set; }

	// связи

	public ICollection<UserGroupRelation> GroupsRelations { get; set; } = [];

	public ICollection<UserGroup> Groups { get; set; } = [];

	[InverseProperty(nameof(AccessRights.User))]
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];
}
