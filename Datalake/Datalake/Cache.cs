using Datalake.Database;
using System;
using System.Collections.Generic;

namespace Datalake
{
	public static class Cache
	{
		public static List<string> Tables { get; set; } = new List<string>();

		public static Dictionary<int, TagHistory> Live { get; set; } = new Dictionary<int, TagHistory>();

		public static void Update() => LastUpdate = DateTime.Now;

		public static DateTime LastUpdate { get; set; }

		public static TagHistory Read(int tagId)
		{
			lock (Live)
			{
				return Live[tagId];
			}
		}

		public static bool IsNew(TagHistory value)
		{
			var old = Read(value.TagId);

			return (old.Quality != value.Quality) || (old.Text != value.Text) || (old.Number != value.Number);
		}

		public static void Write(TagHistory value)
		{
			lock (Live)
			{
				if (Live[value.TagId].Date <= value.Date)
				{
					Live[value.TagId] = value;
				}
			}
		}

		public static void WriteMany(List<TagHistory> values)
		{
			lock (Live)
			{
				foreach (var value in values)
				{
					if (Live[value.TagId].Date <= value.Date)
					{
						Live[value.TagId] = value;
					}
				}
			}
		}
	}
}
