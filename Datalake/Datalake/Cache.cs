using Datalake.Database;
using Datalake.Database.Enums;
using System.Collections.Generic;

namespace Datalake
{
	public static class Cache
	{
		public static Dictionary<int, TagType> Types { get; set; } = new Dictionary<int, TagType>();

		public static Dictionary<int, TagLive> Live { get; set; } = new Dictionary<int, TagLive>();

		public static object Read(int tagId)
		{
			try
			{
				return Live[tagId].Value(Types[tagId]);
			}
			catch
			{
				return 0;
			}
		}
	}
}
