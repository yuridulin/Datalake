using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using LinqToDB.Mapping;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице записей аудита
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName)]
public class Log
{
	const string TableName = "Logs";

	// поля в БД

	/// <summary>
	/// Идентификатор
	/// </summary>
	[Key, Identity, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public long Id { get; set; }

	/// <summary>
	/// Дата записи
	/// </summary>
	[Column, NotNull]
	public DateTime Date { get; set; } = DateFormats.GetCurrentDateTime();

	/// <summary>
	/// Категория
	/// </summary>
	[Column, NotNull]
	public LogCategory Category { get; set; } = LogCategory.Api;

	/// <summary>
	/// Идентификатор связанного объекта
	/// </summary>
	[Column]
	public string? RefId { get; set; }

	/// <summary>
	/// Идентификатор пользователя, совершившего записанное действие
	/// </summary>
	[Column]
	public Guid? UserGuid { get; set; }

	/// <summary>
	/// Тип
	/// </summary>
	[Column, NotNull]
	public LogType Type { get; set; } = LogType.Information;

	/// <summary>
	/// Сообщение о событии
	/// </summary>
	[Column, NotNull]
	public string Text { get; set; } = string.Empty;

	/// <summary>
	/// Пояснения и дополнительная информация
	/// </summary>
	[Column]
	public string? Details { get; set; }

	// связи

	/// <summary>
	/// Пользователь, совершивший записанное действие
	/// </summary>
	public User? Author { get; set; }
}
