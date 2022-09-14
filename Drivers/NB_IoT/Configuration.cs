using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.NB_IoT
{
	public class Configuration
	{
		public int Port { get; set; } = 502;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			string html = "";

			html +=
				Html.Value("TCP порт", nameof(config.Port), config.Port);

			return html;
		}
	}
}