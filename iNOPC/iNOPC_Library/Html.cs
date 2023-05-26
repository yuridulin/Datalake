namespace iNOPC.Library
{
	public static class Html
	{
		public static string Value(string desc, string name, object value)
		{
			return "<div type='value'>" + Input(desc, name, value) + "</div>";
		}

		public static string Input(string desc, string name, object value, int percentWidth = 0)
		{
			var type = value.GetType();

			string style = percentWidth == 0 ? "" : "style='width: " + percentWidth + "%;'";

			// Одиночные кавычки, иначе разрывается строка в исполняемой js функции. Приходящие строки тоже нужно экранировать, если там есть двойные кавычки.
			// Можно добавить ``. Это поможет, но не будет работать в некоторых старых браузерах
			if (type.Equals(typeof(byte)) || type.Equals(typeof(int)) || type.Equals(typeof(uint)) || type.Equals(typeof(ushort)) || type.Equals(typeof(float)))
			{
				return "<span>" + desc + "</span><input name='" + name + "' type='number' value='" + value.ToString().Replace(",", ".") + "' " + style + " />";
			}

			if (type.Equals(typeof(bool)))
			{
				return "<span>" + desc + "</span><input name='" + name + "' type='checkbox' " + ((bool)value ? "checked" : "") + " " + style + " />";
			}

			return "<span>" + desc + "</span><input name='" + name + "' type='text' value='" + value + "' " + style + " />";
		}

		public static string Textarea(string desc, string name, object value)
		{
			return "<div type='value'><span>" + desc + "</span><textarea name='" + name + "'>" + value + "</textarea></div>";
		}

		public static string V(string name, object value, string width = null)
		{
			var type = value.GetType();
			string w = width == null ? "" : " style='width: " + width + "' ";

			if (type.Equals(typeof(byte)) || type.Equals(typeof(int)) || type.Equals(typeof(uint)) || type.Equals(typeof(ushort)))
			{
				return "<input v name='" + name + "' type='number' value='" + value + "' " + w + "/>";
			}

			if (type.Equals(typeof(bool)))
			{
				return "<input v name='" + name + "' type='checkbox' " + ((bool)value ? "checked" : "") + " " + w + "/>";
			}

			return "<input v name='" + name + "' type='text' value='" + value + "' " + w + "/>";
		}
	}
}