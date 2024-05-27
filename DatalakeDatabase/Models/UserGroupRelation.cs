using DatalakeApiClasses.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class UserGroupRelation
{
	const string TableName = "UserGroupRelation";

	// поля в БД

	[Column, NotNull]
	public required Guid UserGuid { get; set; }

	[Column, NotNull]
	public required Guid UserGroupGuid { get; set; }

	[Column, NotNull]
	public UserGroupAccess AccessType { get; set; }

	// связи

	public User? User { get; set; }

	public UserGroup? UserGroup { get; set; }
}

