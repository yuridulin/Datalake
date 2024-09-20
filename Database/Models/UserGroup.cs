using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using NotNullAttribute = LinqToDB.Mapping.NotNullAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class UserGroup
{
	const string TableName = "UserGroups";

	// поля в БД

	[Column, NotNull, Key, PrimaryKey]
	public Guid Guid { get; set; }

	[Column]
	public Guid? ParentGuid { get; set; }

	[Column, NotNull]
	public required string Name { get; set; }

	[Column]
	public string? Description { get; set; }

	// связи

	[NotMapped]
	public ICollection<UserGroup> Childred { get; set; } = [];

	public ICollection<UserGroupRelation> UsersRelations { get; set; } = [];

	public ICollection<User> Users { get; set; } = [];

	[InverseProperty(nameof(AccessRights.UserGroup))]
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];
}
