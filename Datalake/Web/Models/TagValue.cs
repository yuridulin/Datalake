using Datalake.Database;
using System;

namespace Datalake.Web.Models
{
	public class TagValue
	{
		public long Id { get; set; }

		public DateTime Date { get; set; }

		public object Value { get; set; }

		public short Quality { get; set; }
	}
}
