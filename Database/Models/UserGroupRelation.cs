using Datalake.ApiClasses.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Models;

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
	public AccessType AccessType { get; set; }

	// связи

	public User User { get; set; } = null!;

	public UserGroup UserGroup { get; set; } = null!;
}
