using Datalake.Database.Enums;
using System;

namespace Datalake.Web.Models
{
	public class HistoryValue
	{
		public DateTime Date { get; set; } = DateTime.Now;

		public object Value { get; set; }

		public TagQuality Quality { get; set; }

		public TagHistoryUse Using { get; set; }
	}
}
