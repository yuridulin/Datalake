using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.APC_UPS
{
    public class Configuration
    {
        public int Timeout { get; set; } = 60000;

        public string Path { get; set; } = @"C:\iNOPC\Drivers\ApcAccess\apcaccess.exe";

        public string Url { get; set; } = "192.168.0.1:24";

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return
                Html.Value("Время между опросами, мс", nameof(config.Timeout), config.Timeout) +
                Html.Value("Расположение ApcAccess", nameof(config.Path), config.Path) +
                Html.Value("Адрес службы ApcUpsd", nameof(config.Url), config.Url);
        }
    }
}