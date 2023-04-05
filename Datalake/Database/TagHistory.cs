using LinqToDB.Mapping;
using System;

namespace Datalake.Database
{
	[Table(Name = "TagsHistory")]
	public class TagHistory
	{
		[Column]
		public string TagName { get; set; }

		[Column]
		public DateTime Date { get; set; }

		[Column]
		public string Text { get; set; }

		[Column]
		public decimal? Number { get; set; }

		[Column]
		public short Quality { get; set; }
	}
}
