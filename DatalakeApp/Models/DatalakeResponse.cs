namespace DatalakeApp.Models
{
	public class DatalakeResponse
	{
		public DateTime Timestamp { get; set; }

		public DatalakeRecord[] Tags { get; set; } = [];
	}
}
