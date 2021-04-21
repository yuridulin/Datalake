using iNOPC.Library;
using Newtonsoft.Json;

namespace RVP_18
{
	public class Configuration
	{
        public int Timeout { get; set; } = 10000;

        public int ReceiveTimeout { get; set; } = 1000;

        public int DeviceId { get; set; } = 1;

        public string PortName { get; set; } = "COM1";

        public int BaudRate { get; set; } = 38400;

        public byte Parity { get; set; } = 0;

        public byte DataBits { get; set; } = 8;

        public byte StopBits { get; set; } = 1;

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            string html = "";

            html +=
                Html.Value("Интервал опроса, мс", nameof(config.Timeout), config.Timeout) +

                Html.Value("COM порт", nameof(config.PortName), config.PortName) +
                Html.Value("Скорость", nameof(config.BaudRate), config.BaudRate) +
                Html.Value("Кол-во бит данных", nameof(config.DataBits), config.DataBits) +
                Html.Value("Четность (1 = исп.)", nameof(config.Parity), config.Parity) +
                Html.Value("Кол-во стоповых бит", nameof(config.StopBits), config.StopBits) +

                Html.Value("Номер прибора", nameof(config.DeviceId), config.DeviceId);

            return html;
        }
    }
}