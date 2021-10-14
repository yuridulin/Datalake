using iNOPC.Library;
using Newtonsoft.Json;

namespace GranEnergo_CC101
{
	public class Configuration
	{
        public string Ip { get; set; } = "172.22.138.254";

        public int Port { get; set; } = 10250;

        public int CurrentValuesInterval { get; set; } = 60;

        public int DaysValuesInterval { get; set; } = 720;

        public int MonthValuesInterval { get; set; } = 1440;

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return ""
                + Html.Value("IP-адрес устройства", nameof(config.Ip), config.Ip)
                + Html.Value("TCP-порт", nameof(config.Port), config.Port)
                + Html.Value("Интервал опроса текущих значений, мин", nameof(config.CurrentValuesInterval), config.CurrentValuesInterval)
                + Html.Value("Интервал опроса значений за сутки, мин", nameof(config.DaysValuesInterval), config.DaysValuesInterval)
                + Html.Value("Интервал опроса значений за месяц, мин", nameof(config.MonthValuesInterval), config.MonthValuesInterval);
        }

        public static string GetHelp()
		{
            return "";
		}
    }
}