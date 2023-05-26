using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace iNOPC.Drivers.Test_TCP_Console
{
	public class Driver : IDriver
	{
		public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

		public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

		public event LogEvent LogEvent;

		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfig)
		{
			LogEvent("Запуск ...");

			// чтение конфигурации
			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
			}
			catch (Exception e)
			{
				return Err("Конфигурация не прочитана: " + e.Message + "\n" + e.StackTrace);
			}

			Fields.Clear();
			Fields.Add("Answer", new DefField { Value = "", Quality = 192 });

			try
			{
				if (Thread != null)
				{
					Thread.Abort();
					Thread = null;
				}

				Active = true;

				Thread = new Thread(() =>
				{
					while (Active)
					{
						Exchange();
					}
				});
				Thread.Start();

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

			Active = false;

			if (Thread != null)
			{
				Thread.Abort();
				Thread = null;
			}

			if (Client != null && Client.Connected)
			{
				Client.Close();
				Client = null;
			}

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
		}


		// Реализация получения данных

		Configuration Configuration { get; set; } = null;

		TcpClient Client { get; set; }

		NetworkStream Stream { get; set; }

		Thread Thread { get; set; }

		DateTime RequestStart { get; set; }

		bool Active { get; set; }

		bool Connect()
		{
			try
			{
				if (Client?.Connected != true)
				{
					try
					{
						Client?.Close();
						Client = null;

						Stream = null;
					}
					catch { }

					Client = new TcpClient();
					Client.Connect(Configuration.Ip, Configuration.Port);

					if (Client?.Connected == true)
					{
						if (Stream == null)
						{
							Stream = Client.GetStream();
						}
					}
				}

				if (Client?.Connected == true && Stream != null) return true;

				return false;
			}
			catch (Exception e)
			{
				LogEvent("Ошибка подключения: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
				return false;
			}
		}

		void Exchange()
		{
			if (!Active) return;

			RequestStart = DateTime.Now;
			bool isConnected = Connect();

			if (isConnected)
			{
				try { 
				byte[] command = Helpers.StringToBytes(Configuration.CommandInHexString);
				Stream.Write(command, 0, command.Length);
				LogEvent("Tx: " + Helpers.BytesToString(command), LogType.DETAILED);

				DateTime d = DateTime.Now;
				while ((DateTime.Now - d).TotalSeconds < 2 && !Stream.DataAvailable)
				{
					Thread.Sleep(10);
				}

				if (Stream.DataAvailable)
				{
					byte[] answer = new byte[512];
					Stream.Read(answer, 0, answer.Length);
					LogEvent("Rx: " + Helpers.BytesToString(answer), LogType.DETAILED);

						Fields["Answer"].Value = Helpers.BytesToString(answer);
				}
				else
				{
					LogEvent("Данные не вернулись по таймауту", LogType.DETAILED);
				}
				}
				catch (Exception e)
				{
					LogEvent("Ошибка при опросе значений: " + e.Message + "\n" + e.StackTrace, LogType.DETAILED);
				}
			}

			UpdateEvent();

			double ms = (DateTime.Now - RequestStart).TotalMilliseconds;
			if (!Active) return;

			int timeout = Convert.ToInt32((Configuration.CyclicTimeoutInSeconds * 1000 - ms) * 0.001);
			if (timeout > 0) Thread.Sleep(timeout);
		}

		bool Err(string text)
		{
			LogEvent(text, LogType.ERROR);
			return false;
		}
	}
}