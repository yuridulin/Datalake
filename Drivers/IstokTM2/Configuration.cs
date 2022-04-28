using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.IstokTM2
{
    public class Configuration
    {
        // в секундах
        public int CyclicTimeout { get; set; } = 20;

        public byte Port { get; set; } = 1;

        public uint BaudRate { get; set; } = 4800;

        public byte Parity { get; set; } = 0;

        public byte DataBits { get; set; } = 8;

        public byte StopBits { get; set; } = 1;

        public ushort ComTimeout { get; set; } = 1500;


        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            string html = "";

            html +=
                Html.Value("Интервал опроса, с", nameof(config.CyclicTimeout), config.CyclicTimeout) +
                Html.Value("COM порт №", nameof(config.Port), config.Port) +
                Html.Value("Скорость", nameof(config.BaudRate), config.BaudRate) +
                Html.Value("Кол-во бит данных", nameof(config.DataBits), config.DataBits) +
                Html.Value("Четность (1 = исп.)", nameof(config.Parity), config.Parity) +
                Html.Value("Кол-во стоповых бит", nameof(config.StopBits), config.StopBits) +
                Html.Value("Таймаут COM порта, мс", nameof(config.ComTimeout), config.ComTimeout);

            return html;
        }
    }
}