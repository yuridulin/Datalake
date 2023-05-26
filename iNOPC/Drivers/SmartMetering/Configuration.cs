using iNOPC.Library;
using Newtonsoft.Json;

namespace SmartMetering
{
	public class Configuration
	{
		public int ExchangeDelaySeconds { get; set; } = 30;

		public string Endpoint { get; set; } = "172.22.138.20";

		public int Port { get; set; } = 10250;

		public int RxTimeoutSeconds { get; set; } = 2;

		public bool CheckAmperage { get; set; } = true;

		public bool CheckVoltage { get; set; } = true;

		public bool CheckFreq { get; set; } = true;

		public bool CheckActivePower { get; set; } = true;

		public bool CheckReactivePower { get; set; } = true;

		public bool CheckHistory { get; set; } = true;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			string html = "";

			html +=
				Html.Value("Интервал опроса (с)", nameof(config.ExchangeDelaySeconds), config.ExchangeDelaySeconds) + 
				Html.Value("TCP адрес в сети", nameof(config.Endpoint), config.Endpoint) +
				Html.Value("TCP порт", nameof(config.Port), config.Port) +
				Html.Value("Ожидание ответа на команду (с)", nameof(config.RxTimeoutSeconds), config.RxTimeoutSeconds) +
				Html.Value("Получать значения: ток?", nameof(config.CheckAmperage), config.CheckAmperage) +
				Html.Value("Получать значения: напряжение?", nameof(config.CheckVoltage), config.CheckVoltage) +
				Html.Value("Получать значения: частота?", nameof(config.CheckFreq), config.CheckFreq) +
				Html.Value("Получать значения: активная мощность?", nameof(config.CheckActivePower), config.CheckActivePower) +
				Html.Value("Получать значения: реактивная мощность?", nameof(config.CheckReactivePower), config.CheckReactivePower) +
				Html.Value("Получать значения: энергия за день и за месяц?", nameof(config.CheckHistory), config.CheckHistory) +
				"";

			return html;
		}
	}
}
