using Datalake.ApiClasses.Models.Tags;

namespace Datalake.Database.Utilities;

public static class Cache
{
	static object locker = new();

	public static DateTime LastUpdate { get; set; } = DateTime.MinValue;

	public static Dictionary<DateTime, string> Tables { get; set; } = [];

	public static Dictionary<int, TagCacheInfo> Tags { get; set; } = [];

	public static void Update()
	{
		lock (locker)
		{
			LastUpdate = DateTime.Now;
		}
	}
}
