using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице учетных записей
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class User
{
	const string TableName = "Users";

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Column, Key, NotNull]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Тип учетной записи
	/// </summary>
	[Column, NotNull]
	public UserType Type { get; set; }

	/// <summary>
	/// Полное имя
	/// </summary>
	[Column]
	public string? FullName { get; set; } = string.Empty;

	// для локальных

	/// <summary>
	/// Имя для входа
	/// </summary>
	[Column]
	public string? Login { get; set; }

	/// <summary>
	/// Хэш пароля
	/// </summary>
	[Column]
	public string? PasswordHash { get; set; }

	// для статичных

	/// <summary>
	/// Адрес, с которого разрешен доступ
	/// </summary>
	[Column]
	public string? StaticHost { get; set; }

	// для EnergoId

	/// <summary>
	/// Идентификатор в EnergoId
	/// </summary>
	[Column]
	public Guid? EnergoIdGuid { get; set; }


	// связи

	/// <summary>
	/// Список связей с группами пользователей
	/// </summary>
	public ICollection<UserGroupRelation> GroupsRelations { get; set; } = [];

	/// <summary>
	/// Список групп пользователей
	/// </summary>
	public ICollection<UserGroup> Groups { get; set; } = [];

	/// <summary>
	/// Список правил доступа, выданных этой учетной записи
	/// </summary>
	public ICollection<AccessRights> AccessRightsList { get; set; } = [];

	/// <summary>
	/// Список сообщений
	/// </summary>
	public ICollection<Log> Logs { get; set; } = [];
}
