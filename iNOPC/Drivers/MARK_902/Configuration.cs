using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.MARK_902
{
    public class Configuration
    {
        public int Timeout { get; set; } = 10000;

        public string PortName { get; set; } = "COM1";

        public int BaudRate { get; set; } = 9600;

        public int DataBits { get; set; } = 8;

        public int Parity { get; set; } = 1;

        public int StopBits { get; set; } = 1;

        public int ReadTimeout { get; set; } = 500;

        public int WriteTimeout { get; set; } = 500;

        public byte NetworkAddress { get; set; } = 1;

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return
                Html.Value("Таймер опроса, мс", nameof(config.Timeout), config.Timeout) +
                Html.Value("COM порт", nameof(config.PortName), config.PortName) +
                Html.Value("Скорость", nameof(config.BaudRate), config.BaudRate) +
                Html.Value("Кол-во бит данных", nameof(config.DataBits), config.DataBits) +
                Html.Value("Четность (1 = исп.)", nameof(config.Parity), config.Parity) +
                Html.Value("Кол-во стоповых бит", nameof(config.StopBits), config.StopBits) +
                Html.Value("Таймаут на чтение", nameof(config.ReadTimeout), config.ReadTimeout) +
                Html.Value("Таймаут на запись", nameof(config.WriteTimeout), config.WriteTimeout) +
                Html.Value("Сетевой адрес", nameof(config.NetworkAddress), config.NetworkAddress);
        }
    }
}