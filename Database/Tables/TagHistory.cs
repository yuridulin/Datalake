using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using LinqToDB.Mapping;

namespace Datalake.Database.Tables;

/// <summary>
/// Запись в таблице истории значений тегов
/// </summary>
[Table]
public class TagHistory
{
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

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		int hashQuality = Quality.GetHashCode();
		int hashText = Text?.GetHashCode() ?? 0;
		int hashNumber = Number.GetHashCode();

		return hashQuality ^ hashText ^ hashNumber;
	}

	/// <inheritdoc/>
	public override bool Equals(object? obj)
	{
		return obj is TagHistory history && GetHashCode() == history.GetHashCode();
	}
}
