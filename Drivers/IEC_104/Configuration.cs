using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace iNOPC.Drivers.IEC_104
{
    public class Configuration
    {
        public string Host { get; set; } = "192.168.1.1";

        public int Port { get; set; } = 2404;

        public int ConnectionTimeoutT0 { get; set; } = 10;

        public int TimeoutT1 { get; set; } = 15;

        public int TimeoutT2 { get; set; } = 10;

        public int TimeoutT3 { get; set; } = 20;

        public int ReconnectTimeout { get; set; } = 10;

        public int InterrogationTimeout { get; set; } = 30;

        public int SyncClockTimeout { get; set; } = 60;

        public bool UseInterrogation { get; set; } = true;

        public List<Field> NamedFields { get; set; } = new List<Field>();

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            string html = "";

            html +=
                Html.Value("Адрес устройства", nameof(config.Host), config.Host) +
                Html.Value("TCP порт", nameof(config.Port), config.Port) +
                Html.Value("Режим прослушки", nameof(config.UseInterrogation), config.UseInterrogation) +
                Html.Value("T0, с", nameof(config.ConnectionTimeoutT0), config.ConnectionTimeoutT0) +
                Html.Value("T1, с", nameof(config.TimeoutT1), config.TimeoutT1) +
                Html.Value("T2, с", nameof(config.TimeoutT2), config.TimeoutT2) +
                Html.Value("T3, с", nameof(config.TimeoutT3), config.TimeoutT3) +
                Html.Value("Таймаут переподключения, с", nameof(config.ReconnectTimeout), config.ReconnectTimeout) +
                Html.Value("Таймаут команды для синхр. времени, с", nameof(config.SyncClockTimeout), config.SyncClockTimeout) +
                Html.Value("Таймаут цикличного опроса, с", nameof(config.InterrogationTimeout), config.InterrogationTimeout);

            html += "<div type='array' name='" + nameof(config.NamedFields) + "'>"
                + "<span>Именованные поля</span>"
                + "<button onclick='_add(this)'>Добавить</button>";

            foreach (var field in config.NamedFields)
            {
                html += NamedFieldString(field);
            }

            html += "</div>";

            html += "<script>" +
                "function _add(button) { button.insertAdjacentHTML('afterEnd', \"" + NamedFieldString(new Field { Address = 0, Name = "" }) + "\") }" +
                "function _del(button) { button.parentNode.parentNode.removeChild(button.parentNode) }" +
                "</script>";

            return html;

            string NamedFieldString(Field field)
            {
                string select = "<span>Тип</span>"
                    + "<select name='" + nameof(field.Type) + "'>"
                    + Option("Bool")
                    + Option("Float")
                    + "</select>";

                return "<p>"
                    + Html.Input("Адрес", nameof(field.Address), field.Address)
                    + Html.Input("Имя", nameof(field.Name), field.Name)
                    + select
                    + "<button onclick='_del(this)'>Удалить</button>"
                    + "</p>";

                string Option(string type, string name = null)
                {
                    return "<option" + (type == field.Type ? " selected" : "") + " value='" + type + "'>" + (name ?? type) + "</option>";
                }
            }
        }
    }
}