using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.PowerCounters
{
    public class Configuration
    {
        public int CyclicTimeout { get; set; } = 1;

        public string Link { get; set; } = "http://block-state.energo.net/";

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            string html = "";

            html +=
                Html.Value("Интервал опроса, мин", nameof(config.CyclicTimeout), config.CyclicTimeout) +
                Html.Value("Ссылка для получения данных", nameof(config.Link), config.Link);

            return html;
        }
    }
}