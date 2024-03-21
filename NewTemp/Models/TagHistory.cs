using DatalakeDatabase.Enums;
using LinqToDB.Mapping;
using Microsoft.EntityFrameworkCore;

namespace DatalakeDatabase.Models
{
	[Keyless]
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

		// реализация

    public object? GetValue(TagType type) => type switch
    {
      TagType.String => Text,
      TagType.Number => Number,
      TagType.Boolean => Number.HasValue ? Number != 0 : null,
      _ => null,
    };
  }
}
