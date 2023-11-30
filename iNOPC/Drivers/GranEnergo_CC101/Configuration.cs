using iNOPC.Library;
using Newtonsoft.Json;

namespace GranEnergo_CC101
{
	public class Configuration
	{
		public string Ip { get; set; } = "172.22.138.254";

		public int Port { get; set; } = 10250;

		public int CurrentValuesInterval { get; set; } = 60;

		public int PacketTimeout { get; set; } = 200;

		public bool CheckDayValues { get; set; } = true;

		public bool CheckMonthValues { get; set; } = true;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			return ""
				+ Html.Value("IP-адрес устройства", nameof(config.Ip), config.Ip)
				+ Html.Value("TCP-порт", nameof(config.Port), config.Port)
				+ Html.Value("Интервал опроса текущих значений, с", nameof(config.CurrentValuesInterval), config.CurrentValuesInterval)
				+ Html.Value("Ожидание ответного пакета, мс", nameof(config.PacketTimeout), config.PacketTimeout)
				+ Html.Value("Получать значение энергии за сутки", nameof(config.CheckDayValues), config.CheckDayValues)
				+ Html.Value("Получать значение энергии за месяц", nameof(config.CheckMonthValues), config.CheckMonthValues);
		}

		public static string GetHelp()
		{
			return "";
		}
	}
}