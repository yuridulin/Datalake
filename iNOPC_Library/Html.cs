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


            // Одиночные кавычки, иначе разрывается строка в исполняемой js функции. Приходящие строки тоже нужно экранировать, если там есть двойные кавычки.
            // Можно добавить ``. Это поможет, но не будет работать в некоторых старых браузерах
            if (type.Equals(typeof(byte)) || type.Equals(typeof(int)) || type.Equals(typeof(uint)) || type.Equals(typeof(ushort)))
            {
                return "<div title='" + desc + "'><span>" + desc + "</span></div><div><input name='" + name + "' type='number' value='" + value + "' /></div>";
            }

            if (type.Equals(typeof(bool)))
            {
                return "<div title='" + desc + "'><span>" + desc + "</span></div><div><input name='" + name + "' type='checkbox' " + ((bool)value ? "checked" : "") + " /></div>";
            }

            return "<div title='" + desc + "'><span>" + desc + "</span></div><div><input name='" + name + "' type='text' value='" + value + "' /></div>";
        }
    }
}