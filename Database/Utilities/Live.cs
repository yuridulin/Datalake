using Datalake.Database.Models;

namespace Datalake.Database.Utilities;

public static class Live
{
	public static Dictionary<int, TagHistory> Values { get; set; } = [];

	public static List<TagHistory> Read(int[] identifiers)
	{
		return Values
			.Where(x => identifiers.Length == 0 || identifiers.Contains(x.Key))
			.Select(x => x.Value).ToList();
	}

	public static void Write(IEnumerable<TagHistory> values)
	{
		lock (Values)
		{
			foreach (var value in values)
			{
				if (!Values.TryGetValue(value.TagId, out TagHistory? exist))
				{
					Values.Add(value.TagId, value);
				}
				else if (exist.Date <= value.Date)
				{
					Values[exist.TagId] = value;
				}
			}
		}
	}

	public static bool IsNew(TagHistory value)
	{
		if (Values.TryGetValue(value.TagId, out var old))
		{
			return !old.Equals(value);
		}
		else
		{
			return true;
		}
	}
}
