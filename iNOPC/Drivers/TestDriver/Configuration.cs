using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace iNOPC.Drivers.TestDriver
{
	public class Configuration
	{
		public int Tick { get; set; } = 1000;

		public List<Field> Fields { get; set; } = new List<Field>();

		public List<BoolField> Bools { get; set; } = new List<BoolField>();

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			string html = "";

			html += "<div type='value'><span>Тик таймера, мс</span><input name='Tick' type='number' value='" + config.Tick + "' /></div>";
			html += "<div type='array' name='Fields'>"
				+ "<span>Численные параметры</span>"
				+ "<button onclick='_add(this)'>+</button>" +
				string.Join("", config.Fields.Select(x => NumberHtml(x)).ToArray()) +
				"</div>";

			html += "<div type='array' name='Bools'>" +
				"<span>Логические параметры</span>" +
				"<button onclick='_add2(this)'>+</button>" +
				string.Join("", config.Bools.Select(x => BoolHtml(x)).ToArray()) +
				"</div>";

			html += @"<script>
				function _add(button) {
					button.insertAdjacentHTML('afterEnd', """ + NumberHtml(new Field()) + @""")
				}
				function _add2(button) {
					button.insertAdjacentHTML('afterEnd', """ + BoolHtml(new BoolField()) + @""")
				}
				function _del(button) {
					button.parentNode.parentNode.removeChild(button.parentNode)
				}
			</script>";

			return html;

			string NumberHtml(Field field)
			{
				return "<p>" +
					"<span>Имя</span><input name='Name' type='text' value='" + field.Name + "' />" +
					"<span>Адрес</span><input name='Address' type='number' value='" + field.Address + "' />" +
					"<button onclick='_del(this)'>X</button>" +
					"</p>";
			}

			string BoolHtml(BoolField field)
			{
				return "<p>" +
					"<span>Имя</span><input name='Name' type='text' value='" + field.Name + "' />" +
					"<button onclick='_del(this)'>X</button>" +
					"</p>";
			}
		}
	}

	public class Field
	{
		public string Name { get; set; } = "";

		public int Address { get; set; } = 0;
	}

	public class BoolField
	{
		public string Name { get; set; } = "";
	}
}