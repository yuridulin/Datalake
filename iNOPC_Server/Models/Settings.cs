namespace iNOPC.Server.Models
{
	public class Settings
	{
		public int WebConsolePort { get; set; } = 81;

		public int WebConsoleSocketPort { get; set; } = 82;

		public string Name { get; set; } = "iNOPC";

		public string CLSID { get; set; } = "{4537357b-3334-3334-432d-303637382d34}";

		public bool RegisterAsService { get; set; } = true;
	}
}