using Energomera_CE102.Models;
using iNOPC.Library;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Energomera_CE102
{
	public class Configuration
	{
        public string Ip { get; set; } = "172.22.161.7";

        public int Port { get; set; } = 10250;

        public int PacketTimeout { get; set; } = 200;

        public int ExchangeDelay { get; set; } = 500;

        public short DeviceNumber { get; set; } = 0;

        public short StationNumber { get; set; } = 1;

        public int Password { get; set; } = 0;

        public bool SetBadQuality { get; set; } = true;

        // поля

        public bool CheckDailyData { get; set; } = true;

        public bool CheckMonthlyData { get; set; } = true;

        public bool CheckCurrentData { get; set; } = true;

        public bool CheckPower { get; set; } = true;

        public bool CheckDateTime { get; set; } = true;

        public int DailyInterval { get; set; } = 60;

        public int MonthlyInterval { get; set; } = 60;

        public int CurrentInterval { get; set; } = 10;

        public int PowerInterval { get; set; } = 10;

        public int DateTimeInterval { get; set; } = 10;


        public static string GetPage(string json)
		{
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            string html = ""
                + Html.Value("Ip адрес устройства", nameof(Ip), config.Ip)
                + Html.Value("Tcp порт устройства", nameof(Port), config.Port)
                + Html.Value("Идентификационный номер сервера", nameof(StationNumber), config.StationNumber)
                + Html.Value("Идентификационный номер устройства", nameof(DeviceNumber), config.DeviceNumber)
                + Html.Value("Пароль доступа для устройства", nameof(Password), config.Password)
                + Html.Value("Ожидание после подключения, мс", nameof(ExchangeDelay), config.ExchangeDelay)
                + Html.Value("Ожидание ответа, мс", nameof(PacketTimeout), config.PacketTimeout)
                + Html.Value("Зануление данных при ошибках подключения", nameof(SetBadQuality), config.SetBadQuality)
                + "<div><b>Запрашиваемые данные</b>"

                    + "<div><label style='width: 35em'>"
                    + Html.V(nameof(CheckDateTime), config.CheckDateTime) 
                    + "Дата и время с прибора</label><span>Интервал опроса, мин: " 
                    + Html.V(nameof(DateTimeInterval), config.DateTimeInterval, "5em") 
                    + "</span></div>"

                    + "<div><label style='width: 35em'>"
                    + Html.V(nameof(CheckPower), config.CheckPower) 
                    + "Мощность активная, кВт</label><span>Интервал опроса, мин: " 
                    + Html.V(nameof(PowerInterval), config.PowerInterval, "5em") 
                    + "</span></div>"

                    + "<div><label style='width: 35em'>" 
                    + Html.V(nameof(CheckCurrentData), config.CheckCurrentData) 
                    + "Энергия активная потреблённая, текущее значение, кВт*ч</label><span>Интервал опроса, мин: " 
                    + Html.V(nameof(CurrentInterval), config.CurrentInterval, "5em") 
                    + "</span></div>"

                    + "<div><label style='width: 35em'>"
                    + Html.V(nameof(CheckDailyData), config.CheckDailyData) 
                    + "Энергия активная потреблённая на конец суток, кВт*ч</label><span>Интервал опроса, мин: " 
                    + Html.V(nameof(DailyInterval), config.DailyInterval, "5em") 
                    + "</span></div>"

                    + "<div><label style='width: 35em'>"
                    + Html.V(nameof(CheckMonthlyData), config.CheckMonthlyData) 
                    + "Энергия активная потреблённая на конец месяца, кВт*ч</label><span>Интервал опроса, мин: " 
                    + Html.V(nameof(MonthlyInterval), config.MonthlyInterval, "5em") 
                    + "</span></div>"

                + "</div>"
                ;

            return html;
        }

        public static string GetHelp()
        {
            return "";
        }
    }
}