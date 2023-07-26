using Datalake.Database.Enums;
using System;

namespace Datalake.Database.Models
{
	public class TagValue
	{
		public DateTime Date { get; set; }

		public object Value { get; set; }

		public TagQuality Quality { get; set; }

		public TagHistoryUse Using { get; set; }
	}
}
