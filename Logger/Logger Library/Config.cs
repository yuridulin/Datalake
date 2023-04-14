using Newtonsoft.Json;
using System.IO;

namespace Logger_Library
{
	public class Config
	{
		public string Server { get; set; } = string.Empty;

		public string Port { get; set; } = string.Empty;

		public static Config Load(string exePath)
		{
			return JsonConvert.DeserializeObject<Config>(File.ReadAllText(exePath + "\\config.json"));
		}
	}
}
