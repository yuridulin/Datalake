using Datalake.ApiClasses.Constants;
using Datalake.ApiClasses.Enums;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Models;

[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class Log
{
	const string TableName = "Logs";

	// поля в БД

	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long Id { get; set; }

	[Column, NotNull]
	public DateTime Date { get; set; } = DateFormats.GetCurrentDateTime();

	[Column, NotNull]
	public LogCategory Category { get; set; } = LogCategory.Api;

	[Column]
	public string? RefId { get; set; }

	[Column]
	public Guid? UserGuid { get; set; }

	[Column, NotNull]
	public LogType Type { get; set; } = LogType.Information;

	[Column, NotNull]
	public string Text { get; set; } = string.Empty;

	[Column]
	public string? Details { get; set; }
}
