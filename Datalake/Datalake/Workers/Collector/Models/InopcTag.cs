namespace Datalake.Workers.Collector.Models
{
	public class InputTag
	{
		public string Name { get; set; } = "";

		public object Value { get; set; } = null;

		public ushort Quality { get; set; } = 0;
	}
}
