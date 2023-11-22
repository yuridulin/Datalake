using Datalake.Enums;
using System;

namespace Datalake.Models
{
	public class Log
	{
		public DateTime Date { get; set; }

		public string Module { get; set; }

		public string Message { get; set; }

		public LogType Type { get; set; }

		public string ToText()
		{
			return $"{Module}\t{Date:dd.MM.yyyy HH:mm:ss}\t{Type}\t{Message}";
		}

		public static Log FromText(string text)
		{
			string[] parts = text.Split(new char[] { '\t' });

			return new Log
			{
				Module = parts[0],
				Date = DateTime.TryParse(parts[1], out DateTime d)? d: DateTime.MinValue,
				Type = Enum.TryParse(parts[2], out LogType type) ? type : LogType.Error,
				Message = parts[3],
			};
		}
	}
}
