using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Models;

[Keyless, Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class Settings
{
	const string TableName = "Settings";

	// поля в БД

	[Column, NotNull]
	public DateTime LastUpdate { get; set; }

	[Column, NotNull]
	public string KeycloakHost { get; set; } = string.Empty;

	[Column, NotNull]
	public string KeycloakClient { get; set; } = "datalake";

	[Column, NotNull]
	public string EnergoIdApi { get; set; } = string.Empty;

	[Column, NotNull]
	public string InstanceName { get; set; } = string.Empty;
}
