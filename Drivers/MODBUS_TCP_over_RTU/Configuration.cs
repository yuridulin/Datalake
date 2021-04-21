using iNOPC.Drivers.PR20_RS485.Models;
using iNOPC.Library;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace iNOPC.Drivers.PR20_RS485
{
	public class Configuration
    {
        public int CyclicTimeout { get; set; } = 10000;

        public string PortName { get; set; } = "COM1";

        public int BaudRate { get; set; } = 9600;

        public byte Parity { get; set; } = 0;

        public byte DataBits { get; set; } = 8;

        public byte StopBits { get; set; } = 1;

        public List<Field> Fields { get; set; } = new List<Field>();

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            string html = "";

            html +=
                Html.Value("Интервал опроса, мс", nameof(config.CyclicTimeout), config.CyclicTimeout) +
                Html.Value("COM порт", nameof(config.PortName), config.PortName) +
                Html.Value("Скорость", nameof(config.BaudRate), config.BaudRate) +
                Html.Value("Кол-во бит данных", nameof(config.DataBits), config.DataBits) +
                Html.Value("Четность (1 = исп.)", nameof(config.Parity), config.Parity) +
                Html.Value("Кол-во стоповых бит", nameof(config.StopBits), config.StopBits);

            html += "<div type='array' name='" + nameof(config.Fields) + "'>"
                + "<span>Запрашиваемые поля</span>"
                + "<button onclick='_add(this)'>Добавить</button>";

            foreach (var field in config.Fields)
            {
                html += NamedFieldString(field);
            }

            html += "</div>";

            html += "<script>" +
                "function _add(button) { button.insertAdjacentHTML('afterEnd', \"" + NamedFieldString(new Field()) + "\") }" +
                "function _del(button) { button.parentNode.parentNode.removeChild(button.parentNode) }" +
                "</script>";

            return html;

            string NamedFieldString(Field field)
            {
                return "<p>"
                    + Html.Input("Имя", nameof(field.Name), field.Name)
                    + Html.Input("Тип", nameof(field.Type), field.Type)
                    + Html.Input("Адрес", nameof(field.Address), field.Address)
                    + "<button onclick='_del(this)'>Удалить</button>"
                    + "</p>";
            }
        }
    }
}
