using Datalake.Database.Enums;
using LinqToDB.Mapping;

namespace Datalake.Database
{
	[Table]
	public class TagHistory : TagLive
	{
		[Column, Identity]
		public long Id { get; set; }

		[Column]
		public TagHistoryUse Using { get; set; }
	}
}
