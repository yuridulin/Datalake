using Datalake.Database.Extensions;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.Values;
using System.Globalization;

internal static class TagHistoryExtension
{
	static readonly string TrueValue = true.ToString();
	static readonly string OneValue = 1.ToString();

	internal static object? GetTypedValue(this TagHistory tagHistory, TagType type) => type switch
	{
		TagType.String => tagHistory.Text,
		TagType.Number => tagHistory.Number,
		TagType.Boolean => tagHistory.Number.HasValue ? tagHistory.Number != 0 : null,
		_ => null,
	};

	internal static TagHistory CreateFrom(TagCacheInfo tag, ValueWriteRequest request)
	{
		return CreateTagHistory(
				tag.Type,
				tag.Id,
				tag.Frequency,
				tag.ScalingCoefficient,
				request.Date,
				request.Value,
				request.Quality);
	}

	internal static TagHistory CreateFrom(ValueTrustedWriteRequest request)
	{
		return CreateTagHistory(
				request.Tag.Type,
				request.Tag.Id,
				request.Tag.Frequency,
				request.Tag.ScalingCoefficient,
				request.Date,
				request.Value,
				request.Quality);
	}

	private static TagHistory CreateTagHistory(
			TagType tagType,
			int tagId,
			TagFrequency frequency,
			float scalingCoefficient,
			DateTime? date,
			object? value,
			TagQuality? quality)
	{
		var history = new TagHistory
		{
			Date = (date ?? DateFormats.GetCurrentDateTime()).RoundToFrequency(frequency),
			Text = null,
			Number = null,
			Quality = quality ?? TagQuality.Unknown,
			TagId = tagId,
		};

		if (value == null)
			return history;

		string text = value.ToString()!;

		switch (tagType)
		{
			case TagType.String:
				history.Text = text;
				break;

			case TagType.Number:
				if (double.TryParse(text ?? "x", NumberStyles.Float, CultureInfo.InvariantCulture, out double dValue))
				{
					float number = (float)dValue;

					if (scalingCoefficient != 1)
					{
						history.Number = number * scalingCoefficient;
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
