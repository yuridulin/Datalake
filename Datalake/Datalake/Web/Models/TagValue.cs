using Datalake.Database.Enums;
using System;

namespace Datalake.Web.Models
{
	public class TagValue
	{
		public int TagId { get; set; }

		public string TagName { get; set; }

		public DateTime Date { get; set; }

		public TagQuality Quality { get; set; }

		public TagType Type { get; set; }

		public TagHistoryUse Using { get; set; }

		public object Value { get; set; }
	}
}
