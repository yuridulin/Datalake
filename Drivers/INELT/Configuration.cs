using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.INELT
{
    public class Configuration
    {
        public int Timeout { get; set; } = 10000;

        public string Url { get; set; } = "192.168.0.1";

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return
                Html.Value("Таймер опроса, мс", nameof(config.Timeout), config.Timeout) +
                Html.Value("Адрес устройства", nameof(config.Url), config.Url);
        }
    }
}