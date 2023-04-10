using System;

namespace Datalake.Web.Models
{
	public class TagValue
	{
		public long Id { get; set; }

		public DateTime Date { get; set; }

		public string Text { get; set; }

		public decimal? Number { get; set; }
	}
}
