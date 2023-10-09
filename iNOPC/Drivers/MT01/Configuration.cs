using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.MT01
{
	public class Configuration
	{
		public int CyclicIntervalInSeconds { get; set; } = 10;

		public string Endpoint { get; set; } = "";

		public int Port { get; set; } = 10250;

		public byte Address { get; set; } = 0;

		public int ReceiveTimeoutInMilliseconds { get; set; } = 2000;

		public bool CheckCurrent { get; set; } = true;

		public bool CheckParams { get; set; } = true;

		public bool CheckInfo { get; set; } = true;

		public bool CheckDay { get; set; } = true;

		public bool CheckMonth { get; set; } = true;

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			string html = "";

			html +=
				Html.Value("Интервал опроса, с", nameof(config.CyclicIntervalInSeconds), config.CyclicIntervalInSeconds) +
				Html.Value("IP адрес устройства", nameof(config.Endpoint), config.Endpoint) +
				Html.Value("Используемый TCP порт", nameof(config.Port), config.Port) +
				Html.Value("Номер устройства", nameof(config.Address), config.Address) +
				Html.Value("Ожидание ответа, мс", nameof(config.ReceiveTimeoutInMilliseconds), config.ReceiveTimeoutInMilliseconds) +
				"<div>" +
				"<b>Получаемые данные</b>" +
				Html.Value("Текущие", nameof(config.CheckCurrent), config.CheckCurrent) +
				Html.Value("Состояние сети", nameof(config.CheckParams), config.CheckParams) +
				Html.Value("Справочная информация", nameof(config.CheckInfo), config.CheckInfo) +
				Html.Value("Срез за сутки", nameof(config.CheckDay), config.CheckDay) +
				Html.Value("Срез за месяц", nameof(config.CheckMonth), config.CheckMonth) +
				"</div>" +
				"";

			return html;
		}
	}
}