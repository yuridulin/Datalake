using iNOPC.Library;
using Newtonsoft.Json;

namespace GranEnergo_CC101
{
	public class Configuration
	{
		public int ExchangeIntervalMs { get; set; } = 10000;

        public string Ip { get; set; } = "172.22.138.254";

        public int Port { get; set; } = 10250;

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return Html.Value("Интервал опроса, мс", nameof(config.ExchangeIntervalMs), config.ExchangeIntervalMs)
                + Html.Value("IP-адрес устройства", nameof(config.Ip), config.Ip)
                + Html.Value("TCP-порт", nameof(config.Port), config.Port);
        }
    }
}