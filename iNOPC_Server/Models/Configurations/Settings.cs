namespace iNOPC.Server.Models.Configurations
{
	public class Settings
	{
		public int WebConsolePort { get; set; } = 81;

		public int WebConsoleSocketPort { get; set; } = 82;

		public int MathRecalculateMs { get; set; } = 5000;
	}
}