using iNOPC.Library;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Sql
{
	public class Configuration
	{
		public int IntervalInSeconds { get; set; } = 10;

		public string ServerName { get; set; } = "localhost";

		public string DatabaseName { get; set; } = "sqlexpress";

		public string Username { get; set; } = string.Empty;

		public string Password { get; set; } = string.Empty;

		public string Code { get; set; } = string.Empty;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			string html = "";

			html +=
				"<div>" +
				"Драйвер для запроса данных из INSQL." +
				"<br />Код должен возвращать таблицу с двумя столбцами: " +
				"<br />&emsp;1) имя переменной" +
				"<br />&emsp;2) значение" +
				"</div>" +
				Html.Value("Интервал опроса (с)", nameof(config.IntervalInSeconds), config.IntervalInSeconds) +
				Html.Value("Сервер", nameof(config.ServerName), config.ServerName) +
				Html.Value("База данных", nameof(config.DatabaseName), config.DatabaseName) +
				Html.Value("Пользователь", nameof(config.Username), config.Username) +
				Html.Value("Пароль", nameof(config.Password), config.Password) +
				Html.Textarea("Выполняемый код", nameof(config.Code), config.Code);

			return html;
		}
	}

}
