using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.TEM_104
{
	public class Configuration
	{
		public int ExchangeDelaySeconds { get; set; } = 30;

		public string Endpoint { get; set; } = "172.22.138.140";

		public int Port { get; set; } = 10250;

		public int RxTimeoutSeconds { get; set; } = 2;

		public byte DeviceId { get; set; } = 1;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			string html = "";

			html +=
				Html.Value("Интервал опроса (с)", nameof(config.ExchangeDelaySeconds), config.ExchangeDelaySeconds) + 
				Html.Value("TCP адрес в сети", nameof(config.Endpoint), config.Endpoint) +
				Html.Value("TCP порт", nameof(config.Port), config.Port) +
				Html.Value("Номер устройства", nameof(config.DeviceId), config.DeviceId) +
				Html.Value("Ожидание ответа на команду (с)", nameof(config.RxTimeoutSeconds), config.RxTimeoutSeconds) +
				"";

			return html;
		}
	}
}
