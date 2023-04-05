namespace Datalake.Collector.Models
{
	public class InopcTag
	{
		public string Name { get; set; } = "";

		public object Value { get; set; } = null;

		public ushort Quality { get; set; } = 0;

		public decimal? GetNumber()
		{
			if (decimal.TryParse(Value.ToString(), out decimal d)) return d;
			else return null;
		}

		public string GetText()
		{
			return Value.ToString();
		}
	}
}
