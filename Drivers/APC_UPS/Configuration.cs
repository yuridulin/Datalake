using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.APC
{
    public class Configuration
    {
        public int Timeout { get; set; } = 60000;

        public string Name { get; set; } = "UPS";

        public string Url { get; set; } = "192.168.0.1:24";

        public string Exe { get; set; } = @"C:\iNOPC\Drivers\ApcAccess\apcaccess.exe";

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return
                Html.Value("Имя бесперебойника", nameof(config.Name), config.Name) +
                Html.Value("Время между опросами, мс", nameof(config.Timeout), config.Timeout) +
                Html.Value("Путь к ApcUpsd", nameof(config.Exe), config.Exe) +
                Html.Value("Адрес службы ApcUpsd (ip:port)", nameof(config.Url), config.Url);
        }
    }
}