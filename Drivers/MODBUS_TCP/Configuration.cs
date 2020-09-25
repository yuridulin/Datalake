using iNOPC.Drivers.MODBUS_TCP.Models;
using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace iNOPC.Drivers.MODBUS_TCP
{
    public class Configuration
    {
        public int CyclicTimeout { get; set; } = 10000;

        public string Ip { get; set; } = "192.168.0.1";

        public int Port { get; set; } = 502;

        public bool Multicast { get; set; } = true;

        public bool OldByteFirst { get; set; } = false;

		public bool OldRegisterFirst { get; set; } = false;

        public List<Field> Fields { get; set; } = new List<Field>();

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            string html = "";

            html +=
                Html.Value("Интервал опроса, мс", nameof(config.CyclicTimeout), config.CyclicTimeout) +
                Html.Value("IP устройства", nameof(config.Ip), config.Ip) +
                Html.Value("TCP порт", nameof(config.Port), config.Port) +
                Html.Value("Использование групповых запросов", nameof(config.Multicast), config.Multicast) +
                Html.Value("Старшим битом вперед", nameof(config.OldByteFirst), config.OldByteFirst) +
                Html.Value("Старшим регистром вперед", nameof(config.OldRegisterFirst), config.OldRegisterFirst);

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
                string select = "<span>Тип</span>"
                    + "<select name='" + nameof(field.Type) + "'>"
                    + Option(nameof(Byte))
                    + Option(nameof(Int16))
                    + Option(nameof(Int32))
                    + Option(nameof(Int64))
                    + Option(nameof(UInt16))
                    + Option(nameof(UInt32))
                    + Option(nameof(Single))
                    + Option(nameof(Double))
                    + Option(nameof(DateTime))
                    + Option("Int.Int")
                    + Option("TM2Date")
                    + "</select>";

                return "<p>"
                    + Html.Input("Имя", nameof(field.Name), field.Name)
                    + select
                    + Html.Input("Адрес", nameof(field.Address), field.Address)
                    + Html.Input("Делитель", nameof(field.Scale), field.Scale)
                    + "<button onclick='_del(this)'>Удалить</button>"
                    + "</p>";

                string Option(string type, string name = null)
                {
                    return "<option" + (type == field.Type ? " selected" : "") + " value='type'>" + (name ?? type) + "</option>";
                }
            }
        }
    }
}