namespace DatalakeApp.Models
{
	public class LiveRequest
	{
		// проверить использование HashSet

		public int[] Tags { get; set; } = [];

		public string[] TagNames { get; set; } = [];
	}
}
