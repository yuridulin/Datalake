using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.ENERGOMERA_CE303
{
    public class Configuration
    {
        public int Timeout { get; set; } = 10000;

        public string PortName { get; set; } = "COM1";

        public string Devices { get; set; } = "";

        public int RX_Timeout { get; set; } = 400;

        public int TX_Timeout { get; set; } = 200;

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return
                Html.Value("Таймер опроса, мс", nameof(config.Timeout), config.Timeout) +
                Html.Value("COM порт", nameof(config.PortName), config.PortName) +
                Html.Value("Список устройств (через запятую)", nameof(config.Devices), config.Devices) +
                Html.Value("Таймаут RX", nameof(config.RX_Timeout), config.RX_Timeout) +
                Html.Value("Таймаут TX", nameof(config.TX_Timeout), config.TX_Timeout);
        }
    }
}