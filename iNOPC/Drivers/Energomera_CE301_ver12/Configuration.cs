using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.Energomera_CE301_ver12
{
	public class Configuration
	{
		public int ExchangeDelaySeconds { get; set; } = 30;

		public string Endpoint { get; set; } = "172.22.138.137";

		public int Port { get; set; } = 10250;

		public string DeviceNumber { get; set; } = string.Empty;

		public int RxTimeoutMs { get; set; } = 250;

		public bool Is7E1 { get; set; } = true;

		public bool IsReadingEnergyAndData { get; set; } = true;

		public bool IsReadingPowerParams { get; set; } = true;

		public bool SetBadQualityWhenError { get; set; } = true;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			string html = "";

			html +=
				Html.Value("Интервал опроса (с)", nameof(config.ExchangeDelaySeconds), config.ExchangeDelaySeconds) + 
				Html.Value("TCP адрес в сети", nameof(config.Endpoint), config.Endpoint) +
				Html.Value("TCP порт", nameof(config.Port), config.Port) +
				Html.Value("Номер устройства", nameof(config.DeviceNumber), config.DeviceNumber) +
				Html.Value("Таймаут ожидания ответа (мс)", nameof(config.RxTimeoutMs), config.RxTimeoutMs) +
				Html.Value("Программный контроль четности 7-E-1", nameof(config.Is7E1), config.Is7E1) +
				Html.Value("Получение счетчиков энергии", nameof(config.IsReadingEnergyAndData), config.IsReadingEnergyAndData) +
				Html.Value("Получение параметров сети", nameof(config.IsReadingPowerParams), config.IsReadingPowerParams) +
				Html.Value("Сбрасывать значения при ошибке", nameof(config.SetBadQualityWhenError), config.SetBadQualityWhenError) +
				"";

			return html;
		}
	}
}
