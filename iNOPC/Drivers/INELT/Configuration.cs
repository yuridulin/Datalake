using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.INELT
{
	public class Configuration
	{
		public int Timeout { get; set; } = 10000;

		public string Name { get; set; } = "UPS";

		public string PortName { get; set; } = "COM1";

		public int BaudRate { get; set; } = 2400;

		public int DataBits { get; set; } = 8;

		public int StopBits { get; set; } = 1;

		public int Parity { get; set; } = 0;

		public int RxTimeout { get; set; } = 2000;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			return
				Html.Value("Таймер опроса, мс", nameof(config.Timeout), config.Timeout) +
				Html.Value("Имя COM порта", nameof(config.PortName), config.PortName) +
				Html.Value("Скорость", nameof(config.BaudRate), config.BaudRate) +
				Html.Value("Бит данных", nameof(config.DataBits), config.DataBits) +
				Html.Value("Бит стоповых", nameof(config.StopBits), config.StopBits) +
				Html.Value("Четность", nameof(config.Parity), config.Parity) + 
				Html.Value("Ожидание RX после отправки команды, мс", nameof(config.RxTimeout), config.RxTimeout) +
				"";
		}
	}
}