using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице настроек приложения
/// </summary>
[Keyless, Table(TableName), LinqToDB.Mapping.Table(TableName)]
public record class Settings
{
	const string TableName = "Settings";

	/// <summary>Конструктор для LinqToDB</summary>
	public Settings() { }

	// поля в БД

	/// <summary>
	/// Время последнего обновления структуры базы данных
	/// </summary>
	[Column, NotNull]
	public DateTime LastUpdate { get; set; }

	/// <summary>
	/// Сетевое расположение сервера Keycloak EnergoId
	/// </summary>
	[Column, NotNull]
	public string KeycloakHost { get; set; } = string.Empty;

	/// <summary>
	/// Название клиента в Keycloak EnergoId
	/// </summary>
	[Column, NotNull]
	public string KeycloakClient { get; set; } = "datalake";

	/// <summary>
	/// Сетевое расположение сервера API EnergoId
	/// </summary>
	[Column, NotNull]
	public string EnergoIdApi { get; set; } = string.Empty;

	/// <summary>
	/// Название текущей базы данных
	/// </summary>
	[Column, NotNull]
	public string InstanceName { get; set; } = string.Empty;
}
