namespace Datalake.Collector.Models
{
	public class InopcTag
	{
		public string Name { get; set; } = "";

		public object Value { get; set; } = null;

		public ushort Quality { get; set; } = 0;
	}
}
