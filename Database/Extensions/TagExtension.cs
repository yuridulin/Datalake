using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.Tags;
using Datalake.Database.Models;

namespace Datalake.Database.Extensions;

public static class TagExtension
{
	public static TagHistory ToHistory(this TagCacheInfo tag, object? value, ushort qualityRaw)
	{
		var quality = !Enum.IsDefined(typeof(TagQuality), (int)qualityRaw)
			? TagQuality.Unknown
			: (TagQuality)qualityRaw;

		return tag.ToHistory(value, quality);
	}

	public static TagHistory ToHistory(this TagCacheInfo tag, object? value, TagQuality? quality)
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

		if (value == null) return history;

		if (tag.TagType == TagType.String)
		{
			history.Text = value?.ToString();
		}
		else if (float.TryParse(value?.ToString() ?? "x", out float number))
		{
			if (tag.TagType == TagType.Boolean)
			{
				history.Number = number == 1 ? 1 : 0;
			}
			else if (tag.TagType == TagType.Number)
			{
				// вычисление значения на основе шкалирования
				if (tag.ScalingCoefficient != 1)
				{
					history.Number = number * tag.ScalingCoefficient;
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
