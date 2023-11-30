namespace iNOPC.Library
{
	public static class Html
	{
		public static string Value(string desc, string name, object value) 
			=> $"<div type='value'>{Input(desc, name, value)}</div>";

		public static string Input(string desc, string name, object value, string style = "")
		{
			return $"<span>{desc}</span>{V(name, value, style)}";
		}

		public static string Textarea(string desc, string name, object value, string style = "")
		{
			return $"<div type='value'><span>{desc}</span><textarea name='{name}' style='{style}'>{value}</textarea></div>";
		}

		public static string V(string name, object value, string style = "")
		{
			var type = value.GetType();

			if (type.Equals(typeof(byte)) || type.Equals(typeof(uint)) || type.Equals(typeof(ushort)))
			{
				return $"<input v min='0' name='{name}' type='number' value='{value}' style='{style}'/>";
			}

			else if (type.Equals(typeof(int)) || type.Equals(typeof(float)) || type.Equals(typeof(long)))
			{
				return $"<input v name='{name}' type='number' value='{value.ToString().Replace(",", ".")}' style='{style}'/>";
			}

			else if (type.Equals(typeof(bool)))
			{
				return $"<input v name='{name}' type='checkbox' {((bool)value ? "checked" : "")} style='{style}'/>";
			}

			else return $"<input v name='{name}' type='text' value='{value}' style='{style}'/>";
		}
	}
}