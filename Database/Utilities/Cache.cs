using Datalake.ApiClasses.Models.Tags;

namespace Datalake.Database.Utilities;

public static class Cache
{
	static object locker = new();

	public static DateTime LastUpdate { get; set; } = DateTime.MinValue;

	public static void Update()
	{
		lock (locker)
		{
			LastUpdate = DateTime.Now;
		}
	}

	public static Dictionary<DateTime, string> Tables { get; set; } = [];

	public static Dictionary<int, TagCacheInfo> Tags { get; set; } = [];

	public static string? LastTable(DateTime seek) => Tables
		.OrderByDescending(x => x.Key)
		.Where(x => x.Key < seek)
		.Select(x => x.Value)
		.LastOrDefault();

	public static void UpdateTagCache(int id, TagCacheInfo? newTagInfo)
	{
		lock (locker)
		{
			if (newTagInfo == null && Tags.ContainsKey(id))
			{
				Tags.Remove(id);
				Update();
			}
			else if (newTagInfo != null)
			{
				Tags[id] = newTagInfo;
				Update();
			}
		}
	}
}
