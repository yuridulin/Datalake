namespace iNOPC.Library
{
    public static class Helpers
    {
        public static string BytesToString(byte[] bytes)
        {
            string s = "";
            if (bytes.Length > 0)
            {
                for (int i = 0; i < bytes.Length; i++) s += bytes[i].ToString("X2") + " ";
                s = s.Substring(0, s.Length - 1);
            }
            return s;
        }
    }
}