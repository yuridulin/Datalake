namespace iNOPC.Library
{
    public static class Html
    {
        public static string Value(string desc, string name, object value)
        {
            return "<div type='value'>" + Input(desc, name, value) + "</div>";
        }

        public static string Input(string desc, string name, object value)
        {
            var type = value.GetType();

            if (type.Equals(typeof(byte)) || type.Equals(typeof(int)) || type.Equals(typeof(uint)) || type.Equals(typeof(ushort)))
            {
                return "<span>" + desc + "</span><input name='" + name + "' type='number' value='" + value + "' />";
            }

            if (type.Equals(typeof(bool)))
            {
                return "<span>" + desc + "</span><input name='" + name + "' type='checkbox' " + ((bool)value ? "checked" : "") + " />";
            }

            return "<span>" + desc + "</span><input name='" + name + "' type='text' value='" + value + "' />";
        }
    }
}