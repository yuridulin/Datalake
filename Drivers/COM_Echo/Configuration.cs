using iNOPC.Library;
using Newtonsoft.Json;

namespace COM_Echo
{
	public class Configuration
	{
        public string PortName { get; set; } = "COM1";

        public int BaudRate { get; set; } = 9600;

        public byte Parity { get; set; } = 0;

        public byte DataBits { get; set; } = 8;

        public byte StopBits { get; set; } = 1;

        public int Interval { get; set; } = 1;

        public int EchoDelay { get; set; } = 2000;

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return
                Html.Value("Интервал опроса, c", nameof(config.Interval), config.Interval) +
                Html.Value("Ожидание ответа, мс", nameof(config.EchoDelay), config.EchoDelay) +
                Html.Value("COM порт", nameof(config.PortName), config.PortName) +
                Html.Value("Скорость", nameof(config.BaudRate), config.BaudRate) +
                Html.Value("Кол-во бит данных", nameof(config.DataBits), config.DataBits) +
                Html.Value("Четность (1 = исп.)", nameof(config.Parity), config.Parity) +
                Html.Value("Кол-во стоповых бит", nameof(config.StopBits), config.StopBits);
        }
    }
}