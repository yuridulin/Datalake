using Datalake.Database.Views;
using Datalake.PublicApi.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись о сессии учетной записи
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public record class UserSession
{
	const string TableName = "UserSessions";

	/// <summary>Конструктор для LinqToDB</summary>
	public UserSession() { }

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Column, Key, NotNull]
	public Guid UserGuid { get; set; }

	/// <summary>
	/// Дата создания сессии
	/// </summary>
	[Column, NotNull]
	public DateTime Created { get; set; }

	/// <summary>
	/// Время истечения сессии. Если не указана - сессия бессрочная (например, статичная учетная запись)
	/// </summary>
	[Column]
	public DateTime? ExpirationTime { get; set; }

	/// <summary>
	/// Токен доступа
	/// </summary>
	[Column]
	public string Token { get; set; } = null!;

	// связи

	/// <summary>
	/// Связанная учетная запись
	/// </summary>
	public User User { get; set; } = null!;
}
