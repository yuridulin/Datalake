using System;

namespace Datalake.Collector.Models
{
	public class SourceItem
	{
		public string TagName { get; set; }

		public TimeSpan Interval { get; set; }


		DateTime LastRequest { get; set; } = DateTime.MinValue;

		public bool IsTimed(DateTime now)
		{
			return Interval == TimeSpan.Zero || now - LastRequest >= Interval;
		}

		public void Reset(DateTime now)
		{
			LastRequest = now;
		}
	}
}
