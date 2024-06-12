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
	public string EnergoIdHost { get; set; } = string.Empty;
}
