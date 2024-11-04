using Datalake.Database.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице связей учетных записей и групп пользователей
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class UserGroupRelation
{
	const string TableName = "UserGroupRelation";

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Column, Key, Identity]
	public int Id { get; set; }

	/// <summary>
	/// Идентификатор учетной записи
	/// </summary>
	[Column, NotNull]
	public required Guid UserGuid { get; set; }

	/// <summary>
	/// Идентификатор группы пользователей
	/// </summary>
	[Column, NotNull]
	public required Guid UserGroupGuid { get; set; }

	/// <summary>
	/// Уровень доступа пользователя к группе
	/// </summary>
	[Column, NotNull]
	public AccessType AccessType { get; set; }

	// связи

	/// <summary>
	/// Пользователь
	/// </summary>
	public User User { get; set; } = null!;

	/// <summary>
	/// Группа пользователей
	/// </summary>
	public UserGroup UserGroup { get; set; } = null!;
}
