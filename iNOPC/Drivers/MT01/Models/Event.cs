using System;

namespace iNOPC.Drivers.MT01.Models
{
	public class Event
	{
		public Event(byte[] bytes, int start)
		{
			Date = new DateTime(2000 + bytes[start + 5], bytes[start + 4], bytes[start + 3]);
			Date.AddHours(bytes[start + 2]);
			Date.AddMinutes(bytes[start + 1]);
			Date.AddSeconds(bytes[start]);

			Code = bytes[start + 6];

			Value = BitConverter.ToUInt16(bytes, start + 7) / 10f;
		}

		public DateTime Date { get; set; }

		public byte Code { get; set; }

		public float Value { get; set; }

		public string LogText()
		{
			string log = "Неизвестное событие";

			switch (Code)
			{
				case 10: log = "Появление напряжения на фазам А, B, C"; break;
				case 11: log = "Появление напряжения на фазе А"; break;
				case 12: log = "Появление напряжения на фазе B"; break;
				case 13: log = "Появление напряжения на фазе C"; break;

				case 20: log = "Пропадание напряжения по фазам А, B, C"; break;
				case 21: log = "Пропадание напряжения по фазе А"; break;
				case 22: log = "Пропадание напряжения по фазе B"; break;
				case 23: log = "Пропадание напряжения по фазе C"; break;

				case 31: log = "Низкий уровень напряжения на фазе А, значение: " + Value; break;
				case 32: log = "Низкий уровень напряжения на фазе B, значение: " + Value; break;
				case 33: log = "Низкий уровень напряжения на фазе C, значение: " + Value; break;

				case 34: log = "Высокий уровень напряжения на фазе А, значение: " + Value; break;
				case 35: log = "Высокий уровень напряжения на фазе B, значение: " + Value; break;
				case 36: log = "Высокий уровень напряжения на фазе C, значение: " + Value; break;

				case 37: log = "Уровень напряжения ниже нормы на фазе А, значение: " + Value; break;
				case 38: log = "Уровень напряжения ниже нормы на фазе B, значение: " + Value; break;
				case 39: log = "Уровень напряжения ниже нормы на фазе C, значение: " + Value; break;

				case 40: log = "Уровень напряжения выше нормы на фазе А, значение: " + Value; break;
				case 41: log = "Уровень напряжения выше нормы на фазе B, значение: " + Value; break;
				case 42: log = "Уровень напряжения выше нормы на фазе C, значение: " + Value; break;

				case 128: log = "Синхронизация времени"; break;
				case 129: log = "Изменение настроек"; break;
			}

			return Date + " " + log;
		}
	}
}
