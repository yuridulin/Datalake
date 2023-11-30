using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Drivers.IstokTM2
{
	public class Driver : IDriver
	{
		public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

		public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

		public event LogEvent LogEvent;

		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfig)
		{
			LogEvent("Запуск...");

			Fields = new Dictionary<string, DefField>();

			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
			}
			catch (Exception e)
			{
				LogEvent("Конфигурация не прочитана: " + e.Message, LogType.ERROR);
				return false;
			}

			// Проверка допустимости значений конфигурации


			

			// Настройка таймера

			Timer = new Timer(Configuration.CyclicTimeout * 1000);
			Timer.Elapsed += (s, e) => Exchange();
			Timer.Start();

			Fields["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 };
			UpdateEvent();

			LogEvent("Мониторинг активен");

			Task.Run(Exchange);

			return true;
		}

		public void Stop()
		{
			LogEvent("Остановка...");

			try
			{
				Timer?.Stop();
				Timer = null;
			} 
			catch { }

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			Fields[fieldName].Value = value;
			LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
		}

		// реализация

		Configuration Configuration { get; set; } = new Configuration();

		Timer Timer { get; set; }

		ComPort Com;

		void Exchange()
		{
			try
			{
				byte bytes;

				// Установка параметров
				bytes = Istok.GetComPortConfig(out Com);

				Com.Number = Configuration.Port;
				Com.BaudRate = Configuration.BaudRate;
				//Com.StopBits = Configuration.StopBits;
				//Com.Parity = Configuration.Parity;
				Com.Timeout = Configuration.ComTimeout;

				bytes = Istok.SetComPortConfig(Com, true);


				// Подключение
				bytes = Istok.InitComPort();
				LogEvent("Init COM: " + bytes, LogType.DETAILED);

				bytes = Istok.GetComFuncError();
				LogEvent("GetComFuncError: " + bytes, LogType.DETAILED);


				// Получение времени прибора
				Istok.ReadDateTime(1, out double time);
				var date = DateTime.FromOADate(time);
				LogEvent("Time = " + time + ", Date = " + date, LogType.DETAILED);


				// Чтение реальных данных
				LogEvent("Чтение текущих данных", LogType.DETAILED);
				Istok.ReadOperativeData(1, out Operative data);


				// Чтение данных за час
				date = DateTime.Now.AddHours(-1);
				LogEvent("Чтение часовых данных: " + date + " (" + date.ToOADate() + ")", LogType.DETAILED);
				//var hours = new List<TM2Data>();
				//for (byte i = 1; i < 5; i++)
				//{
				//    Istok.ReadHourData(1, i, date.ToOADate(), out TM2Data hourData);
				//    hours.Add(hourData);
				//}



				// Чтение данных за час
				date = DateTime.Now.AddDays(-1).Date.AddHours(23).AddMinutes(55);
				LogEvent("Чтение часовых данных: " + date + " (" + date.ToOADate() + ")", LogType.DETAILED);
				//var days = new List<TM2Data>();
				//for (byte i = 1; i < 5; i++)
				//{
				//    Istok.ReadDayData(1, i, date.ToOADate(), out TM2Data dayData);
				//    days.Add(dayData);
				//}


				// Отключение
				bytes = Istok.FreeComPort();
				LogEvent("Free COM: " + bytes, LogType.DETAILED);


				// Заполнение полей
				lock (Fields)
				{
					byte i = 1;

					//Update("ColdSource.h", data.ColdSourceData.h);
					Update("ColdSource.P", data.ColdSourceData.P);
					Update("ColdSource.t", data.ColdSourceData.t);
					Update("ColdSource.Patm", data.ColdSourceData.Patm);

					for (i = 0; i < data.PointData.Length; i++)
					{
						Update("Point " + (i + 1) + ".G", data.PointData[i].G);
						Update("Point " + (i + 1) + ".h", data.PointData[i].h);
						Update("Point " + (i + 1) + ".P", data.PointData[i].P);
						Update("Point " + (i + 1) + ".Q", data.PointData[i].Q);
						Update("Point " + (i + 1) + ".r", data.PointData[i].r);
						Update("Point " + (i + 1) + ".t", data.PointData[i].t);
						Update("Point " + (i + 1) + ".V", data.PointData[i].V);
						Update("Point " + (i + 1) + ".dP", data.PointData[i].dP);
					}

					//i = 1;
					//foreach (var hour in hours)
					//{
					//    foreach (var rec in hour.Data)
					//    {
					//        Update("Hour.Point" + i + ".G", rec.G);
					//        Update("Hour.Point" + i + ".M", rec.M);
					//        Update("Hour.Point" + i + ".P", rec.P);
					//        Update("Hour.Point" + i + ".Patm", rec.Patm);
					//        Update("Hour.Point" + i + ".Q", rec.Q);
					//        Update("Hour.Point" + i + ".t", rec.t);
					//    }

					//    i++;
					//}

					//i = 1;
					//foreach (var day in days)
					//{
					//    foreach (var rec in day.Data)
					//    {
					//        Update("Day.Point" + i + ".G", rec.G);
					//        Update("Day.Point" + i + ".M", rec.M);
					//        Update("Day.Point" + i + ".P", rec.P);
					//        Update("Day.Point" + i + ".Patm", rec.Patm);
					//        Update("Day.Point" + i + ".Q", rec.Q);
					//        Update("Day.Point" + i + ".t", rec.t);
					//    }

					//    i++;
					//}

					Update("DeviceTime", date.ToString("HH:mm:ss"));
					Update("Time", DateTime.Now.ToString("HH:mm:ss"));
				}

				UpdateEvent();
			}
			catch (Exception e)
			{
				LogEvent("Ошибка: " + e.Message, LogType.ERROR);

				// Заполнение полей
				lock (Fields)
				{
					foreach (var key in Fields.Keys)
					{
						Fields[key].Quality = 0;
					}

					Update("Time", DateTime.Now.ToString("HH:mm:ss"));
				}

				UpdateEvent();
			}
		}

		void Update(string name, object value, ushort q = 192)
		{
			if (Fields.ContainsKey(name))
			{
				Fields[name].Value = value;
				Fields[name].Quality = q;
			}
			else
			{
				Fields.Add(name, new DefField { Name = name, Quality = q, Value = value });
			}
		}
	}
}