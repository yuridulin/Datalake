using Datalake.Database.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице правил доступа
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class AccessRights
{
	const string TableName = "AccessRights";

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	/// <summary>
	/// Идентификатор пользователя, на которого выдано правило
	/// </summary>
	[Column]
	public Guid? UserGuid { get; set; }

	/// <summary>
	/// Идентификатор группы, на которую выдано правило
	/// </summary>
	[Column]
	public Guid? UserGroupGuid { get; set; }

	/// <summary>
	/// Это правило глобальное для всего приложения?
	/// </summary>
	[Column, NotNull]
	public bool IsGlobal { get; set; }

	/// <summary>
	/// Идентификатор тега, на который действует правило
	/// </summary>
	[Column]
	public int? TagId { get; set; }

	/// <summary>
	/// Идентификатор источника, на который действует правило
	/// </summary>
	[Column]
	public int? SourceId { get; set; }

	/// <summary>
	/// Идентификатор блока, на который действует правило
	/// </summary>
	[Column]
	public int? BlockId { get; set; }

	/// <summary>
	/// Выданный уровень доступа
	/// </summary>
	[Column]
	public AccessType AccessType { get; set; } = AccessType.NoAccess;

	// связи

	/// <summary>
	/// Пользователь, на которого выдано правило
	/// </summary>
	public User? User { get; set; }

	/// <summary>
	/// Группа пользователей, на которую выдано правило
	/// </summary>
	public UserGroup? UserGroup { get; set; }

	/// <summary>
	/// Тег, на который действует правило
	/// </summary>
	public Tag? Tag { get; set; }

	/// <summary>
	/// Источник, на который действует правило
	/// </summary>
	public Source? Source { get; set; }

	/// <summary>
	/// Блок, на который действует правило
	/// </summary>
	public Block? Block { get; set; }
}
