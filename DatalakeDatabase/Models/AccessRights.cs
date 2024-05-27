using DatalakeApiClasses.Models.Abstractions;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace DatalakeDatabase.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class AccessRights : IRights
{
	const string TableName = "AccessRights";

	// поля в БД

	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	#region Keys

	[Column]
	public Guid? UserGuid { get; set; }

	[Column]
	public Guid? UserGroupGuid { get; set; }

	[Column]
	public int? TagId { get; set; }

	[Column]
	public int? SourceId { get; set; }

	[Column]
	public int? BlockId { get; set; }

	#endregion

	#region Tags

	[Column]
	public bool? HasAccessToTag { get; set; }

	[Column]
	public bool? CanManageTag { get; set; }

	[Column]
	public bool? CanWriteToTag { get; set; }

	#endregion

	#region Blocks

	[Column]
	public bool? HasAccessToBlock { get; set; }

	[Column]
	public bool? CanManageBlock { get; set; }

	[Column]
	public bool? CanManageBlockTags { get; set; }

	#endregion

	#region Sources

	[Column]
	public bool? HasAccessToSource { get; set; }

	[Column]
	public bool? CanManageSource { get; set; }

	[Column]
	public bool? CanManageSourceTags { get; set; }

	#endregion

	#region Administration 

	[Column]
	public bool? CanControlAccess { get; set; }

	[Column]
	public bool? CanViewSystemTags { get; set; }

	[Column]
	public bool? CanViewLogs { get; set; }

	#endregion

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
