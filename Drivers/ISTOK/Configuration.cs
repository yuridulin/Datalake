using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.ISTOK
{
    public class Configuration
    {
        public int Timeout { get; set; } = 10000;

        public byte Number { get; set; } = 1;

        public uint BaudRate { get; set; } = 9600;

        public byte Parity { get; set; } = 1;

        public byte StopBits { get; set; } = 1;

        public ushort ComTimeout { get; set; } = 1000;

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return
                Html.Value("Таймер опроса, мс", nameof(config.Timeout), config.Timeout) +
                Html.Value("Номер COM порта", nameof(config.Number), config.Number) +
                Html.Value("Скорость", nameof(config.BaudRate), config.BaudRate) +
                Html.Value("Четность (1 = исп.)", nameof(config.Parity), config.Parity) +
                Html.Value("Стоповые биты", nameof(config.StopBits), config.StopBits) +
                Html.Value("Таймаут подключения, мс", nameof(config.ComTimeout), config.ComTimeout);
        }
    }
}