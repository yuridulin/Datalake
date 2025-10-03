using Datalake.Contracts.Public.Enums;
using Datalake.DataService.Extensions;
using Datalake.Shared.Application;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Tags;
using System.Globalization;
using Datalake.Data.Host.Abstractions;
using Datalake.Data.Host.Models.Values;
using Datalake.Data.Host.Database.Entities;

namespace Datalake.Data.Host.Factories;

[Singleton]
public class TagHistoryFactory : ITagHistoryFactory
{
	public TagHistory CreateFrom(TagCacheInfo tag, ValueWriteRequest request)
	{
		return CreateTagHistory(
			tag.Type,
			tag.Resolution,
			tag.ScalingCoefficient,
			tag.Id,
			request.Date,
			request.Value,
			request.Quality);
	}

	public TagHistory CreateFrom(ValueTrustedWriteRequest request)
	{
		return CreateTagHistory(
			request.Tag.Type,
			request.Tag.Resolution,
			request.Tag.ScalingCoefficient,
			request.Tag.Id,
			request.Date,
			request.Value,
			request.Quality);
	}

	private static TagHistory CreateTagHistory(
		TagType type,
		TagResolution resolution,
		float scaling,
		int id,
		DateTime? date,
		object? value,
		TagQuality? quality)
	{
		DateTime resolvedDate = (date ?? DateFormats.GetCurrentDateTime()).RoundByResolution(resolution);
		TagQuality actualQuality = quality ?? TagQuality.Unknown;

		if (value != null)
		{
			string? text = value.ToString();

			switch (type)
			{
				case TagType.String:
					return new TagHistory(id, resolvedDate, text, null, actualQuality);

				case TagType.Number:
					if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double dValue))
						return new TagHistory(id, resolvedDate, TagQuality.Bad);

					float numberValue = (float)dValue * scaling;
					return new TagHistory(id, resolvedDate, null, numberValue, actualQuality);

				case TagType.Boolean:
					return new TagHistory(id, resolvedDate, null, text == One || text == True ? 1 : 0, actualQuality);
			}
		}

		return new TagHistory(id, resolvedDate, actualQuality);
	}

	/// <summary>
	/// Значение истины
	/// </summary>
	static readonly string True = true.ToString();

	/// <summary>
	/// Значение единицы
	/// </summary>
	static readonly string One = 1.ToString();
}
