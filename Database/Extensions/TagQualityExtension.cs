using Datalake.Database.Enums;

namespace Datalake.Database.Extensions;

internal static class TagQualityExtension
{
	internal static TagQuality GetLOCFValue(this TagQuality quality)
	{
		if ((int)quality < (int)TagQuality.Good)
			return TagQuality.Bad_LOCF;
		else
			return TagQuality.Good_LOCF;
	}
}