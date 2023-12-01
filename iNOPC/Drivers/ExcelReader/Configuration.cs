using iNOPC.Library;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExcelReader
{
	public class Configuration
	{
		public string FilePath { get; set; } = "";

		public int MinutesReadInterval { get; set; } = 1;

		public List<CellInfo> Cells { get; set; } = new List<CellInfo>();

		public static string GetPage(string json)
		{
			var config = JsonConvert.DeserializeObject<Configuration>(json);

			var html = ""
				+ Html.Value("Путь к файлу", nameof(config.FilePath), config.FilePath)
				+ Html.Value("Интервал между чтением, мин", nameof(config.MinutesReadInterval), config.MinutesReadInterval);

			html += $@"<table type='array' name='{nameof(config.Cells)}'>
				<thead>
					<tr>
						<th style='width: 3em;'>Вкл.</th>
						<th style='width: 8em;'>№ листа</th>
						<th style='width: 8em;'>№ строки</th>
						<th style='width: 8em;'>№ столбца</th>
						<th>Название тега</th>
						<th style='width: 8em;'></th>
					</tr>
				</thead>
				<tbody>";

			foreach (var cell in config.Cells)
			{
				html += cell.ToConfiguration();
			}

			html += "<tr><td colspan='5'><button onclick='_add(this)'>+</button></td></tr></tbody></table>";

			html += "<script>" +
				"function _add(button) { button.parentNode.parentNode.insertAdjacentHTML('beforeBegin', \"" + new CellInfo().ToConfiguration() + "\") }" +
				"function _del(button) { button.parentNode.parentNode.parentNode.removeChild(button.parentNode.parentNode) }" +
			"</script>";

			return html;
		}
	}
}
