using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.Test_TCP_Console
{
	public class Configuration
	{
		public int CyclicTimeoutInSeconds { get; set; } = 5;

		public string Ip { get; set; } = "172.22.138.138";

		public int Port { get; set; } = 10250;

		public string CommandInHexString { get; set; } = "";

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			string html = "";

			html +=
				Html.Value("Интервал опроса, с", nameof(config.CyclicTimeoutInSeconds), config.CyclicTimeoutInSeconds) +
				Html.Value("IP устройства", nameof(config.Ip), config.Ip) +
				Html.Value("TCP порт", nameof(config.Port), config.Port) +
				Html.Value("Команда (HEX через пробел)", nameof(config.CommandInHexString), config.CommandInHexString);

			return html;
		}
	}
}