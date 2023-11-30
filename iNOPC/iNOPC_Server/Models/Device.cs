using iNOPC.Library;
using iNOPC.Server.Storage;
using iNOPC.Server.Web;
using System;
using System.Collections.Generic;
using System.Linq;

namespace iNOPC.Server.Models
{
	public class Device
	{
		// Параметры конфигурации

		public string Name { get; set; } = "UnnamedDevice";

		public bool Active { get; set; } = false;

		public bool AutoStart { get; set; } = false;

		public string Configuration { get; set; } = "{}";


		// Рабочие параметры

		public int Id { get; set; } = 0;

		public string DriverName { get; set; }

		public int DriverId { get; set; }

		public List<Log> Logs { get; set; } = new List<Log>();

		private IDriver InnerDriver { get; set; } = null;


		// Методы взаимодействия

		public void Start()
		{
			// Получение типа драйвера из сборки
			var driver = Program.Configuration.Drivers.FirstOrDefault(x => x.Name == DriverName);
			if (driver == null)
			{
				Log("Драйвер " + DriverName + " не найден", LogType.ERROR);
				return;
			}

			var type = driver.DriverType;
			if (type == null)
			{
				Log("Драйвер " + DriverName + " не содержит тип Driver", LogType.ERROR);
				return;
			}

			// Пересоздание объекта драйвера при необходимости
			try
			{
				if (InnerDriver == null || InnerDriver.GetType() != type)
				{
					InnerDriver = (IDriver)Activator.CreateInstance(type);

					// Так как объект только создан, вешаем обработчики событий
					InnerDriver.LogEvent += Log;
					InnerDriver.UpdateEvent += Update;
				}
			}
			catch (Exception e)
			{
				Log("Драйвер не создан: " + e.Message, LogType.ERROR);
				return;
			}

			// Запуск мониторинга
			try
			{
				Active = InnerDriver.Start(Configuration);

				Http.Update();
			}
			catch (Exception e)
			{
				try
				{
					InnerDriver.Stop();
				}
				catch (Exception) { }

				Log("Ошибка при старте опроса: " + e.Message + ": " + e.StackTrace, LogType.ERROR);
			}
		}

		public void Stop()
		{
			try
			{
				InnerDriver?.Stop();

				Active = false;

				Http.Update();
			}
			catch (Exception e)
			{
				Log("Ошибка при остановке опроса: " + e.Message, LogType.WARNING);
			}
		}

		public void Log(string text, LogType type = LogType.REGULAR)
		{
			lock (Logs)
			{
				if (Logs.Count >= 100)
				{
					Logs.RemoveAt(0);
				}
				Logs.Add(new Log
				{
					Date = DateTime.Now,
					Text = text,
					Type = type
				});

				if (type == LogType.ERROR)
				{
					Program.Log("Error: " + text + "\nDriver: " + Name);
				}
			}
		}

		public void Update()
		{
			var fields = Fields();

			foreach (var field in fields)
			{
				OPC.Write(DriverName + '.' + Name + '.' + field.Key, field.Value.Value, field.Value.Quality);
			}
		}

		public void Write(string fieldName, object value)
		{
			if (!Active) return;
			if (InnerDriver == null) return;

			InnerDriver.Write(fieldName, value);
		}

		public string GetConfigurationPage()
		{
			// Получение типа драйвера из сборки
			var driver = Program.Configuration.Drivers.FirstOrDefault(x => x.Name == DriverName);
			if (driver == null)
			{
				Log("Драйвер " + DriverName + " не найден", LogType.ERROR);
				return null;
			}

			if (driver.ConfigurationPage == null)
			{
				Log("Драйвер " + DriverName + " не содержит конфигурационной страницы", LogType.ERROR);
				return null;
			}

			return (string)driver.ConfigurationPage.Invoke(null, new object[] { Configuration });
		}

		public Dictionary<string, DefField> Fields()
		{
			if (InnerDriver != null)
			{
				lock (InnerDriver.Fields)
				{
					return InnerDriver.Fields.ToDictionary(x => x.Key, x => x.Value ?? new DefField());
				}
			}
			else return new Dictionary<string, DefField>();
		}
	}
}