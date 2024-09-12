using Datalake.ApiClasses.Enums;
using LinqToDB.Mapping;

namespace Datalake.Database.Models;

[Table]
public class TagHistory
{
	[Column, NotNull]
	public int TagId { get; set; }

	[Column, NotNull]
	public DateTime Date { get; set; } = DateTime.Now;

	[Column, Nullable]
	public string? Text { get; set; } = null;

	[Column, Nullable]
	public float? Number { get; set; } = null;

	[Column, NotNull]
	public TagQuality Quality { get; set; } = TagQuality.Good;

	[Column, NotNull]
	public TagUsing Using { get; set; } = TagUsing.Basic;



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
		return obj is TagHistory history && _cachedHashCode == history._cachedHashCode;
	}
}
