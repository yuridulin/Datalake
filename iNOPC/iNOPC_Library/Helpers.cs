using System;

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

		public static byte[] StringToBytes(string s)
		{
			string[] raw = s.Split(' ');
			byte[] bytes = new byte[raw.Length];
			for (byte i = 0; i < raw.Length; i++)
			{
				bytes[i] = Convert.ToByte(raw[i], 16);
			}
			return bytes;
		}
	}
}