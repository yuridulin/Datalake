using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.ASK_Binary
{
	public class Configuration
	{
		public int Timeout { get; set; } = 10000;

		public string Path { get; set; } = "";

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			return
				Html.Value("Интервал опроса, мин", nameof(config.Timeout), config.Timeout) +
				Html.Value("Путь к папке с файлами", nameof(config.Path), config.Path) +
				"";
		}
	}
}