using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class User
{
	const string TableName = "Users";

	// поля в БД

	[Column, Key, NotNull]
	public required Guid Guid { get; set; }

	[Column, NotNull]
	public UserType Type { get; set; }

	[Column]
	public string? FullName { get; set; } = string.Empty;

	// для локальных

	[Column]
	public string? Login { get; set; }

	[Column]
	public string? PasswordHash { get; set; }

	// для статичных

	[Column]
	public string? StaticHost { get; set; }

	// для EnergoId

	[Column]
	public Guid? EnergoIdGuid { get; set; }


	// связи

	public ICollection<UserGroupRelation> GroupsRelations { get; set; } = [];

	public ICollection<UserGroup> Groups { get; set; } = [];

	public ICollection<AccessRights> AccessRightsList { get; set; } = [];
}
