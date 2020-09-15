using iNOPC.Drivers.MODBUS_RTU.Models;
using iNOPC.Library;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace iNOPC.Drivers.MODBUS_RTU
{
    public class Configuration
    {
        public int CyclicTimeout { get; set; }

        public byte SlaveId { get; set; } = 0;

        public bool Multicast { get; set; } = false;

        public int ReceiveTimeout { get; set; } = 1000;

        public bool OldByteFirst { get; set; } = false;

        public bool OldRegisterFirst { get; set; } = false;

        public string PortName { get; set; }

        public int BaudRate { get; set; }

        public byte Parity { get; set; }

        public byte DataBits { get; set; }

        public byte StopBits { get; set; }

        public int ReadTimeout { get; set; }

        public int WriteTimeout { get; set; }

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
                Html.Value("Кол-во стоповых бит", nameof(config.StopBits), config.StopBits) +
                Html.Value("Таймаут на чтение, мс", nameof(config.ReadTimeout), config.ReadTimeout) +
                Html.Value("Таймаут на запись, мс", nameof(config.WriteTimeout), config.WriteTimeout) +
                Html.Value("Адрес устройства", nameof(config.SlaveId), config.SlaveId) +
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
                return "<p>"
                    + Html.Input("Имя", nameof(field.Name), field.Name)
                    + Html.Input("Тип", nameof(field.Type), field.Type)
                    + Html.Input("Команда", nameof(field.CommandCode), field.CommandCode)
                    + Html.Input("Адрес", nameof(field.Address), field.Address)
                    + "<button onclick='_del(this)'>Удалить</button>"
                    + "</p>";
            }
        }
    }
}