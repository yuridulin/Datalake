using Datalake.ApiClasses.Models.Tags;
using Datalake.Database.Models;

namespace Datalake.Database;

public static class Cache
{
	public static DateTime LastUpdate { get; set; } = DateTime.MinValue;

	public static List<string> Tables { get; set; } = [];

	public static Dictionary<int, TagCacheInfo> Tags { get; set; } = [];

	public static Dictionary<int, TagHistory> LiveValues { get; set; } = [];

	public static void LiveValuesSet(IEnumerable<TagHistory> values)
	{
		lock (LiveValues)
		{
			foreach (var value in values)
			{
				if (!LiveValues.TryGetValue(value.TagId, out TagHistory? exist))
				{
					LiveValues.Add(value.TagId, value);
				}
				else if (exist.Date <= value.Date)
				{
					LiveValues[exist.TagId] = value;
				}
			}
		}
	}
}
