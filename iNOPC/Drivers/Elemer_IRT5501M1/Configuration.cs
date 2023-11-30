using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.Elemer_IRT5501M1
{
	public class Configuration
	{
		public int Timeout { get; set; } = 5000;

		public string PortName { get; set; } = "COM1";

		public int BaudRate { get; set; } = 9600;

		public byte Parity { get; set; } = 0;

		public byte DataBits { get; set; } = 8;

		public byte StopBits { get; set; } = 1;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			return Html.Value("Интервал опроса, мс", nameof(config.Timeout), config.Timeout) +
				Html.Value("COM порт", nameof(config.PortName), config.PortName) +
				Html.Value("Скорость", nameof(config.BaudRate), config.BaudRate) +
				Html.Value("Кол-во бит данных", nameof(config.DataBits), config.DataBits) +
				Html.Value("Четность (1 = исп.)", nameof(config.Parity), config.Parity) +
				Html.Value("Кол-во стоповых бит", nameof(config.StopBits), config.StopBits);
		}
	}
}