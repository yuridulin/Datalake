namespace iNOPC.Drivers.ENERGOMERA_CE303
{
    public static class Helper
    {
        public static decimal ToValue(this string raw, int sector = 1)
        {
            for (int i = 0; i < sector; i++)
            {
                raw = raw.Substring(raw.IndexOf('(') + 1);
            }
            raw = raw.Substring(0, raw.IndexOf(')'));
            raw = raw.Replace('.', ',');

            return decimal.Parse(raw);
        }
    }
}