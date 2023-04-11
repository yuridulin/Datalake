using LinqToDB.Mapping;
using System;

namespace Datalake.Database
{
	[Table(Name = "TagsLive")]
	public class TagLive
	{
		[Column]
		public string TagName { get; set; }

		[Column]
		public DateTime Date { get; set; }

		[Column]
		public string Text { get; set; } = null;

		[Column]
		public decimal? Number { get; set; } = null;

		[Column]
		public short Quality { get; set; }

		public object Value(TagType type)
		{
			switch (type)
			{
				case TagType.Number: return Number;
				case TagType.Boolean: return Number != 0;
				default: return Text;
			}
		}
	}
}
