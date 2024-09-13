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

		if (value == null)
			return history;

		string text = value.ToString()!;

		switch (tag.TagType)
		{
			case TagType.String:
				history.Text = text;
				break;

			case TagType.Number:
				if (float.TryParse(text ?? "x", out float number))
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
				break;

			case TagType.Boolean:
				history.Number = text == "1" || text == "true" ? 1 : 0;
				break;
		}

		return history;
	}
}
