using DatalakeDatabase.Enums;
using DatalakeDatabase.Models;

namespace DatalakeDatabase.Extensions
{
	public static class TagExtension
	{
		public static TagHistory ToHistory(this Tag tag, object? value, ushort quality)
		{
			var history = new TagHistory
			{
				Date = DateTime.UtcNow,
				Text = null,
				Number = null,
				Quality = TagQuality.Unknown,
				TagId = tag.Id,
				Using = TagUsing.Basic,
			};

			if (tag.Type == TagType.String)
			{
				if (value is string v)
				{
					history.Text = v;
				}
			}

			else if (tag.Type == TagType.Boolean)
			{
				if (value is bool v)
				{
					history.Text = v ? "true" : "false";
					history.Number = v ? 1 : 0;
				}
			}

			else if (tag.Type == TagType.Number)
			{
				float? raw = null;
				if (float.TryParse(value?.ToString(), out float d))
				{
					raw = d;
				}

				history.Number = raw;

				// вычисление значения на основе шкалирования
				if (tag.Type == TagType.Number && raw.HasValue && tag.IsScaling)
				{
					history.Number = raw.Value * ((tag.MaxEu - tag.MinEu) / (tag.MaxRaw - tag.MinRaw));
				}
			}

			history.Quality = !Enum.IsDefined(typeof(TagQuality), (int)quality)
				? TagQuality.Unknown
				: (TagQuality)quality;

			return history;
		}
	}
}
