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
		}

		public DateTime Date { get; set; }

		public byte Code { get; set; }

		public string OpcText()
		{
			switch (Code)
			{
				case 10: return "GetVoltageOnAll";
				case 11: return "GetVoltageOnA";
				case 12: return "GetVoltageOnB";
				case 13: return "GetVoltageOnC";

				case 20: return "LostVoltageOnAll";
				case 21: return "LostVoltageOnA";
				case 22: return "LostVoltageOnB";
				case 23: return "LostVoltageOnC";

				case 31: return "LowVoltageOnA";
				case 32: return "LowVoltageOnB";
				case 33: return "LowVoltageOnC";

				case 34: return "HighVoltageOnA";
				case 35: return "HighVoltageOnB";
				case 36: return "HighVoltageOnC";

				case 37: return "BelowNormalVoltageOnA";
				case 38: return "BelowNormalVoltageOnB";
				case 39: return "BelowNormalVoltageOnC";

				case 40: return "AboveNormalVoltageOnA";
				case 41: return "AboveNormalVoltageOnB";
				case 42: return "AboveNormalVoltageOnC";

				case 128: return "TimeSync";
				case 129: return "ChangeSettings";

				default: return "Unknown";
			}
		}

		public string LogText()
		{
			switch (Code)
			{
				case 10: return "Появление напряжения на фазам А, B, C";
				case 11: return "Появление напряжения на фазе А";
				case 12: return "Появление напряжения на фазе B";
				case 13: return "Появление напряжения на фазе C";

				case 20: return "Пропадание напряжения по фазам А, B, C";
				case 21: return "Пропадание напряжения по фазе А";
				case 22: return "Пропадание напряжения по фазе B";
				case 23: return "Пропадание напряжения по фазе C";

				case 31: return "Низкий уровень напряжения на фазе А";
				case 32: return "Низкий уровень напряжения на фазе B";
				case 33: return "Низкий уровень напряжения на фазе C";

				case 34: return "Высокий уровень напряжения на фазе А";
				case 35: return "Высокий уровень напряжения на фазе B";
				case 36: return "Высокий уровень напряжения на фазе C";

				case 37: return "Уровень напряжения ниже нормы на фазе А";
				case 38: return "Уровень напряжения ниже нормы на фазе B";
				case 39: return "Уровень напряжения ниже нормы на фазе C";

				case 40: return "Уровень напряжения выше нормы на фазе А";
				case 41: return "Уровень напряжения выше нормы на фазе B";
				case 42: return "Уровень напряжения выше нормы на фазе C";

				case 128: return "Синхронизация времени";
				case 129: return "Изменение настроек";

				default: return "Неизвестное событие";
			}
		}
	}
}
