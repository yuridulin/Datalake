using Newtonsoft.Json;
using System.Collections.Generic;

namespace iNOPC.Drivers.TestDriver
{
    public class Configuration
    {
        public int Tick { get; set; } = 1000;

        public List<Field> Fields { get; set; } = new List<Field>();

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            string html = "";

            html += "<div type='value'><span>Тик таймера, мс</span><input name='Tick' type='number' value='" + config.Tick + "' /></div>";
            html += "<div type='array' name='Fields'>"
                + "<span>Поля</span>"
                + "<button onclick='_add(this)'>+</button>";

            foreach (var field in config.Fields)
            {
                html += "<p>"
                 + "<span>Имя</span><input name='Name' type='text' value='" + field.Name + "' />"
                 + "<span>Адрес</span><input name='Address' type='number' value='" + field.Address + "' />"
                 + "<button onclick='_del(this)'>X</button>" 
                 + "</p>";
            }

            html += "</div>";

            html += @"<script>
                function _add(button) {
                    button.insertAdjacentHTML('afterEnd', ""<p>Имя: <input name='Name' type='text' value='' />Адрес: <input name='Address' type='number' value='0' /><button onclick='_del(this)'>X</button></p>"")
                }
                function _del(button) {
                    button.parentNode.parentNode.removeChild(button.parentNode)
                }
            </script>";

            return html;
        }
    }

    public class Field
    {
        public string Name { get; set; } = "";

        public int Address { get; set; } = 0;
    }
}