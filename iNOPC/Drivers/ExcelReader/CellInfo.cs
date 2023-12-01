using iNOPC.Library;

namespace ExcelReader
{
	public class CellInfo
	{
		public bool IsActive { get; set; } = false;

		public string Name { get; set; } = string.Empty;

		public uint Sheet { get; set; } = 0;

		public uint Row { get; set; } = 0;

		public uint Column { get; set; } = 0;

		public string ToConfiguration()
		{
			return 
				$"<tr value>" +
					$"<td>{Html.V(nameof(IsActive), IsActive, "width: 100%")}</td>" +
					$"<td>{Html.V(nameof(Sheet), Sheet, "width: 100%")}</td>" +
					$"<td>{Html.V(nameof(Row), Row, "width: 100%")}</td>" +
					$"<td>{Html.V(nameof(Column), Column, "width: 100%")}</td>" +
					$"<td>{Html.V(nameof(Name), Name, "width: 100%")}</td>" +
					$"<td><button onclick='_del(this)'>x</button></td>" +
				$"</tr>";
		}
	}
}