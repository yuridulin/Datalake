using Datalake.ApiClasses.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class AccessRights
{
	const string TableName = "AccessRights";

	// поля в БД

	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Column]
	public Guid? UserGuid { get; set; }

	[Column]
	public Guid? UserGroupGuid { get; set; }

	[Column, NotNull]
	public bool IsGlobal { get; set; }

	[Column]
	public int? TagId { get; set; }

	[Column]
	public int? SourceId { get; set; }

	[Column]
	public int? BlockId { get; set; }

	[Column]
	public AccessType AccessType { get; set; } = AccessType.NoAccess;

	// связи

	[ForeignKey(nameof(UserGuid))]
	public User? User { get; set; }

	[ForeignKey(nameof(UserGroupGuid))]
	public UserGroup? UserGroup { get; set; }

	[ForeignKey(nameof(TagId))]
	public Tag? Tag { get; set; }

	[ForeignKey(nameof(SourceId))]
	public Source? Source { get; set; }

	[ForeignKey(nameof(BlockId))]
	public Block? Block { get; set; }
}
