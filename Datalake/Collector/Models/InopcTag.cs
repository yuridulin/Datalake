using Datalake.Database.Enums;
using System;

namespace Datalake.Collector.Models
{
	public class InopcTag
	{
		public string Name { get; set; } = "";

		public object Value { get; set; } = null;

		public ushort Quality { get; set; } = 0;

		public decimal? GetNumber()
		{
			if (decimal.TryParse(Value?.ToString(), out decimal d)) return d;
			else return null;
		}

		public string GetText()
		{
			return Value?.ToString();
		}

		public TagQuality GetQuality()
		{
			if (!Enum.IsDefined(typeof(TagQuality), Quality))
				return TagQuality.Unknown;

			return (TagQuality)Quality;
		}
	}
}
