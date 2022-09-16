using iNOPC.Library;
using Newtonsoft.Json;

namespace SmartMetering
{
	public class Configuration
	{
		public int ExchangeDelaySeconds { get; set; } = 4;

		public string Endpoint { get; set; } = "172.22.138.20";

		public int Port { get; set; } = 10250;

		public int DeviceId { get; set; } = 80106864;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			string html = "";

			html +=
				Html.Value("Интервал опроса (с)", nameof(config.ExchangeDelaySeconds), config.ExchangeDelaySeconds) + 
				Html.Value("TCP адрес в сети", nameof(config.Endpoint), config.Endpoint) +
				Html.Value("TCP порт", nameof(config.Port), config.Port) +
				Html.Value("Адрес счетчика", nameof(config.DeviceId), config.DeviceId) +
				"";

			return html;
		}
	}
}
