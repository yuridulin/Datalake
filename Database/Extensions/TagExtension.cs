using Datalake.ApiClasses.Enums;
using Datalake.Database.Models;

namespace Datalake.Database.Extensions;

public static class TagExtension
{
	public static TagHistory ToHistory(this Tag tag, object? value, ushort qualityRaw)
	{
		var quality = !Enum.IsDefined(typeof(TagQuality), (int)qualityRaw)
			? TagQuality.Unknown
			: (TagQuality)qualityRaw;

		return tag.ToHistory(value, quality);
	}

	public static TagHistory ToHistory(this Tag tag, object? value, TagQuality? quality)
	{
		var history = new TagHistory
		{
			Date = DateTime.Now,
			Text = null,
			Number = null,
			Quality = quality ?? TagQuality.Unknown,
			TagId = tag.Id,
			Using = TagUsing.Basic,
		};


		if (tag.Type == TagType.String)
		{
			history.Text = value?.ToString();
		}

		else
		{
			float? number = float.TryParse(value?.ToString() ?? "x", out float d) ? d : null;

			if (tag.Type == TagType.Boolean)
			{
				history.Number = number.HasValue ? (number.Value == 1 ? 1 : 0) : 0;

				history.Text = history.Number != 0 ? "true" : "false";
			}

			else if (tag.Type == TagType.Number)
			{
				// вычисление значения на основе шкалирования
				if (tag.Type == TagType.Number && number.HasValue && tag.IsScaling)
				{
					history.Number = number.Value * ((tag.MaxEu - tag.MinEu) / (tag.MaxRaw - tag.MinRaw));
				}
				else
				{
					history.Number = number;
				}
			}
		}

		return history;
	}
}
