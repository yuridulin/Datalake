using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using ColumnAttribute = LinqToDB.Mapping.ColumnAttribute;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице истории значений тегов
/// </summary>
[Table(TableName), LinqToDB.Mapping.Table(TableName), Keyless]
public record class TagHistory
{
	const string TableName = "TagsHistory";

	// поля в БД

	/// <summary>
	/// Идентификатор тега
	/// </summary>
	[Column, NotNull]
	public int TagId { get; set; }

	/// <summary>
	/// Дата
	/// </summary>
	[Column, NotNull]
	public DateTime Date { get; set; } = DateFormats.GetCurrentDateTime();

	/// <summary>
	/// Текстовое значение
	/// </summary>
	[Column, Nullable, DataType(LinqToDB.DataType.Text)]
	public string? Text { get; set; } = null;

	/// <summary>
	/// Числовое значение
	/// </summary>
	[Column, Nullable]
	public float? Number { get; set; } = null;

	/// <summary>
	/// Флаг качества
	/// </summary>
	[Column, NotNull]
	public TagQuality Quality { get; set; } = TagQuality.Good;
}
