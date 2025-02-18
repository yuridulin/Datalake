using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Extensions;
using Datalake.Database.Models.Tags;
using Datalake.Database.Models.Values;
using Datalake.Database.Tables;
using System.Globalization;

internal static class TagHistoryExtension
{
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
				tag.TagType,
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
				request.Tag.TagType,
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
			TagId = tagId,
			Quality = quality ?? TagQuality.Unknown,
			Text = null,
			Number = null
		};

		if (value == null)
			return history;

		switch (tagType)
		{
			case TagType.String:
				history.Text = value.ToString();
				break;

			case TagType.Number:
				if (TryGetFloat(value, out float number))
				{
					history.Number = number * scalingCoefficient;
				}
				break;

			case TagType.Boolean:
				history.Number = ConvertToBoolean(value) ? 1 : 0;
				break;
		}

		return history;
	}

	private static bool TryGetFloat(object value, out float result)
	{
		switch (value)
		{
			case float f:
				result = f;
				return true;
			case double d:
				result = (float)d;
				return true;
			case int i:
				result = i;
				return true;
			case long l:
				result = l;
				return true;
			case string s when float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed):
				result = parsed;
				return true;
			default:
				result = 0;
				return false;
		}
	}

	private static bool ConvertToBoolean(object value)
	{
		switch (value)
		{
			case bool b:
				return b;
			case int i:
				return i != 0;
			case long l:
				return l != 0;
			case float f:
				return f != 0;
			case double d:
				return d != 0;
			case string s when bool.TryParse(s, out bool parsedBool):
				return parsedBool;
			case string s when float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedFloat):
				return parsedFloat != 0;
			default:
				return false;
		}
	}
}
