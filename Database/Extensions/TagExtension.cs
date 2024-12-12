using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Models.Tags;
using Datalake.Database.Tables;

namespace Datalake.Database.Extensions;

internal static class TagExtension
{
	static readonly string TrueValue = true.ToString();
	static readonly string OneValue = 1.ToString();

	internal static TagHistory ToHistory(this TagCacheInfo tag, object? value, ushort qualityRaw)
	{
		var quality = !Enum.IsDefined(typeof(TagQuality), (int)qualityRaw)
			? TagQuality.Unknown
			: (TagQuality)qualityRaw;

		return tag.ToHistory(value, quality);
	}

	internal static TagHistory ToHistory(this TagCacheInfo tag, object? value, TagQuality? quality)
	{
		var history = new TagHistory
		{
			Date = DateFormats.GetCurrentDateTime(),
			Text = null,
			Number = null,
			Quality = quality ?? TagQuality.Unknown,
			TagId = tag.Id,
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
				history.Number = text == OneValue || text == TrueValue ? 1 : 0;
				break;
		}

		return history;
	}
}
