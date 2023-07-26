using Datalake.Database;
using System;
using System.Collections.Generic;

namespace Datalake
{
	public static class Cache
	{
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
				Live[value.TagId] = value;
			}
		}
	}
}
