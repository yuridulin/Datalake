using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.ECOM_3000
{
    public class Configuration
    {
        public int Timeout { get; set; } = 60000;

        public string Url { get; set; } = "192.168.0.1:24";

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return
                Html.Value("Время между опросами, мс", nameof(config.Timeout), config.Timeout) +
                Html.Value("Адрес сервера", nameof(config.Url), config.Url);
        }
    }
}