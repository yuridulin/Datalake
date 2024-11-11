using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using NotNullAttribute = LinqToDB.Mapping.NotNullAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице групп пользователей
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class UserGroup
{
	const string TableName = "UserGroups";

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Column, NotNull, Key, PrimaryKey]
	public Guid Guid { get; set; }

	/// <summary>
	/// Идентификатор родительской группы
	/// </summary>
	[Column]
	public Guid? ParentGuid { get; set; }

	/// <summary>
	/// Название
	/// </summary>
	[Column, NotNull]
	public required string Name { get; set; }

	/// <summary>
	/// Описание
	/// </summary>
	[Column]
	public string? Description { get; set; }

	// связи

	/// <summary>
	/// Родительская группа
	/// </summary>
	public UserGroup? Parent { get; set; }

	/// <summary>
	/// Список подгрупп
	/// </summary>
	public ICollection<UserGroup> Children { get; set; } = [];

	/// <summary>
	/// Список связей с пользователями
	/// </summary>
	public ICollection<UserGroupRelation> UsersRelations { get; set; } = [];

	/// <summary>
	/// Список пользователей
	/// </summary>
	public ICollection<User> Users { get; set; } = [];

	/// <summary>
	/// Список правил доступа, выданных этой группе
	/// </summary>
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];
}
