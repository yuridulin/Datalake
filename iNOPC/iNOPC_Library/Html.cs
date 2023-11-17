namespace iNOPC.Library
{
	public static class Html
	{
		public static string Value(string desc, string name, object value) 
			=> $"<div type='value'>{Input(desc, name, value)}</div>";

		public static string Input(string desc, string name, object value, string style = "")
		{
			var type = value.GetType();

			// Одиночные кавычки, иначе разрывается строка в исполняемой js функции. Приходящие строки тоже нужно экранировать, если там есть двойные кавычки.
			// Можно добавить ``. Это поможет, но не будет работать в некоторых старых браузерах
			if (type.Equals(typeof(byte)) || type.Equals(typeof(int)) || type.Equals(typeof(uint)) || type.Equals(typeof(ushort)) || type.Equals(typeof(float)))
			{
				return $"<span>{desc}</span><input name='{name}' type='number' value='{value.ToString().Replace(",", ".")}' style='{style}' />";
			}

			else if (type.Equals(typeof(bool)))
			{
				return $"<span>{desc}</span><input name='{name}' type='checkbox' {((bool)value ? "checked" : "")} style='{style}' />";
			}
			else
			{
				return $"<span>{desc}</span><input name='{name}' type='text' value='{value}' style='{style}' />";
			}
		}

		public static string Textarea(string desc, string name, object value, string style = "")
		{
			return $"<div type='value'><span>{desc}</span><textarea name='{name}' style='{style}'>{value}</textarea></div>";
		}

		public static string V(string name, object value, string style = "")
		{
			var type = value.GetType();

			if (type.Equals(typeof(byte)) || type.Equals(typeof(int)) || type.Equals(typeof(uint)) || type.Equals(typeof(ushort)))
			{
				return $"<input v name='{name}' type='number' value='{value}' style='{style}'/>";
			}

			else if (type.Equals(typeof(bool)))
			{
				return $"<input v name='{name}' type='checkbox' {((bool)value ? "checked" : "")} style='{style}'/>";
			}

			else return $"<input v name='{name}' type='text' value='{value}' style='{style}'/>";
		}
	}
}