using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.MT01
{
	public class Configuration
	{
		public int CyclicIntervalInSeconds { get; set; } = 10;

		public byte Address { get; set; } = 0;

		public int Port { get; set; } = 1;

		public int BaudRate { get; set; } = 9600;

		public byte Parity { get; set; } = 0;

		public byte DataBits { get; set; } = 8;

		public byte StopBits { get; set; } = 1;

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
				Html.Value("Адрес устройства", nameof(config.Address), config.Address) +
				"<div>" +
				"<b>COM порт</b>" +
				Html.Value("Номер", nameof(config.Port), config.Port) +
				Html.Value("Скорость", nameof(config.BaudRate), config.BaudRate) +
				Html.Value("Четность", nameof(config.Parity), config.Parity) +
				Html.Value("Биты данных", nameof(config.DataBits), config.DataBits) +
				Html.Value("Стоповые биты", nameof(config.StopBits), config.StopBits) +
				Html.Value("Ожидание ответа, мс", nameof(config.ReceiveTimeoutInMilliseconds), config.ReceiveTimeoutInMilliseconds) +
				"</div>" +
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