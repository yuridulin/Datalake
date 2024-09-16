using Datalake.ApiClasses.Models.Tags;

namespace Datalake.Database.Utilities;

public static class Cache
{
	public static DateTime LastUpdate { get; set; } = DateTime.MinValue;

	public static Dictionary<DateTime, string> Tables { get; set; } = [];

	public static Dictionary<int, TagCacheInfo> Tags { get; set; } = [];

	public static string? LastTable(DateTime seek) => Tables
		.OrderByDescending(x => x.Key)
		.Where(x => x.Key < seek)
		.Select(x => x.Value)
		.LastOrDefault();
}
