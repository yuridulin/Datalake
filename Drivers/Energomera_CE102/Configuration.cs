using Newtonsoft.Json;

namespace Energomera_CE102
{
	public class Configuration
	{
		public static string GetPage(string json)
		{
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return ""
                //+ Html.Value("Интервал опроса текущих значений, с", nameof(config.CurrentValuesInterval), config.CurrentValuesInterval)
                ;
        }

        public static string GetHelp()
        {
            return "";
        }
    }
}