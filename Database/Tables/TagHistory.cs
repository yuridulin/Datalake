using Datalake.Database.Constants;
using Datalake.Database.Enums;
using LinqToDB.Mapping;

namespace Datalake.Database.Tables;

[Table]
public class TagHistory
{
	[Column, NotNull]
	public int TagId { get; set; }

	[Column, NotNull]
	public DateTime Date { get; set; } = DateFormats.GetCurrentDateTime();

	[Column, Nullable, DataType(LinqToDB.DataType.Text)]
	public string? Text { get; set; } = null;

	[Column, Nullable]
	public float? Number { get; set; } = null;

	[Column, NotNull]
	public TagQuality Quality { get; set; } = TagQuality.Good;



	[System.ComponentModel.DataAnnotations.Schema.NotMapped]
	private int? _cachedHashCode;

	public override int GetHashCode()
	{
		if (!_cachedHashCode.HasValue)
		{
			int hashQuality = Quality.GetHashCode();
			int hashText = Text?.GetHashCode() ?? 0;
			int hashNumber = Number.GetHashCode();

			_cachedHashCode = hashQuality ^ hashText ^ hashNumber;
		}

		return _cachedHashCode.Value;
	}

	public override bool Equals(object? obj)
	{
		return obj is TagHistory history && GetHashCode() == history.GetHashCode();
	}
}
