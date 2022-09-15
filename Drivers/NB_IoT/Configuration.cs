using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.NB_IoT
{
	public class Configuration
	{
		public int Port { get; set; } = 10240;

		public int SecondsToDisconnect { get; set; } = 2;

		public int MinutesForGoodValues { get; set; } = 5;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			string html = "";

			html +=
				Html.Value("TCP порт", nameof(config.Port), config.Port) +
				Html.Value("Таймаут на чтение данных (с)", nameof(config.SecondsToDisconnect), config.SecondsToDisconnect) +
				Html.Value("Время достоверности значений (мин)", nameof(config.MinutesForGoodValues), config.MinutesForGoodValues);

			return html;
		}
	}
}