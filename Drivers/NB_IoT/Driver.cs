using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iNOPC.Drivers.NB_IoT
{
	public class Driver : IDriver
	{
		public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

		public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

		public event LogEvent LogEvent;

		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfiguration)
		{
			LogEvent("Запуск ...");

			// чтение конфигурации
			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfiguration);
			}
			catch (Exception e)
			{
				return Err("Конфигурация не прочитана: " + e.Message + "\n" + e.StackTrace);
			}

			Fields.Clear();
			Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 });

			UpdateEvent();

			try
			{
				IsActive = true;

				Task.Run(Listen);

				LogEvent("Мониторинг запущен");

				return true;
			}
			catch (Exception e)
			{
				return Err("Неизвестная ошибка: " + e.Message + "\n" + e.StackTrace);
			}
		}

		public void Stop()
		{
			LogEvent("Остановка ...");

			IsActive = false;
			Listener.Stop();

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			throw new System.NotImplementedException();
		}

		// Реализация получения данных

		Configuration Configuration { get; set; }

		TcpListener Listener { get; set; }

		bool IsActive { get; set; } = false;

		bool Err(string text)
		{
			LogEvent(text, LogType.ERROR);
			return false;
		}

		void Listen()
		{
			Listener = new TcpListener(IPAddress.Any, Configuration.Port);
			Listener.Start();

			while (IsActive)
			{
				try
				{
					var client = Listener.AcceptTcpClient();

					LogEvent("Подключен клиент. Выполнение запроса...");

					var stream = client.GetStream();
					var bytes = new List<byte>();
					bool isReadingDone = Task
						.Run(() => 
						{
							while (stream.DataAvailable)
							{
								int b = stream.ReadByte();
								if (b < 0) break;
								bytes.Add((byte)b);
							}
						})
						.Wait(TimeSpan.FromSeconds(3));

					if (!isReadingDone)
					{
						LogEvent("Чтение потока прервано по таймауту");
					}

					LogEvent("Получено: " + Helpers.BytesToString(bytes.ToArray()));

					stream.Close();
					client.Close();

					LogEvent("Клиент отключён");
				}
				catch
				{
					LogEvent("Ошибка в задаче Listener", LogType.ERROR);
				}
			}
		}
	}
}
