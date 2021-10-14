using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace GranEnergo_CC101
{
	public class Driver : IDriver
	{
		public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

		public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

		public event LogEvent LogEvent;
		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfiguration)
		{
			LogEvent("Запуск мониторинга...", LogType.REGULAR);
			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfiguration);
			}
			catch
			{
				LogEvent("Конфигурация не прочитана из json", LogType.ERROR);
				return false;
			}

			try
			{
				Fields.Clear();
			}
			catch
			{
				LogEvent("Конфигурация не прочитана из json", LogType.ERROR);
				return false;
			}

			try
			{
				int interval = 10;

				if (Configuration.CurrentValuesInterval > 0 && Configuration.CurrentValuesInterval < interval) 
					interval = Configuration.CurrentValuesInterval;

				ExchangeTimer = new Timer(interval * 1000);
				ExchangeTimer.Elapsed += (s, e) => { Exchange(); };
			}
			catch
			{
				LogEvent("Конфигурация не прочитана из json", LogType.ERROR);
				return false;
			}

			LastCurrent = DateTime.MinValue;
			LastDay = DateTime.MinValue;

			IsDriverActive = true;
			IsExchangeRunning = false;
			ExchangeTimer.Start();

			LogEvent("Мониторинг запущен", LogType.REGULAR);
			UpdateEvent();
			Task.Run(Exchange);

			return true;
		}

		public void Stop()
		{
			LogEvent("Остановка мониторинга...", LogType.REGULAR);

			IsDriverActive = false;
			try { ExchangeTimer.Stop(); } catch { }

			LogEvent("Мониторинг остановлен", LogType.REGULAR);
		}

		public void Write(string fieldName, object value)
		{
			throw new NotImplementedException();
		}


		Configuration Configuration { get; set; }

		Timer ExchangeTimer { get; set; }

		bool IsDriverActive { get; set; }

		bool IsExchangeRunning { get; set; }

		DateTime LastCurrent { get; set; } = DateTime.MinValue;

		DateTime LastDay { get; set; } = DateTime.MinValue;

		void Exchange()
		{
			if (!IsDriverActive) return;
			if (IsExchangeRunning) return;

			IsExchangeRunning = true;

			// определение необходимости опроса прибора
			bool needCurrent = false, needDay = false;
			DateTime now = DateTime.Now;

			if (Configuration.CurrentValuesInterval > 0 && (now - LastCurrent).TotalSeconds > Configuration.CurrentValuesInterval)
			{
				LastCurrent = now;
				needCurrent = true;
				LogEvent("Чтение текущих значений", LogType.DETAILED);
			}
			if (now.Date != LastDay.Date && now.TimeOfDay >= DateTime.Parse("01.01.2000 03:00:00").TimeOfDay)
			{
				LastDay = now;
				needDay = true;
				if (Configuration.CheckDayValues) LogEvent("Чтение значений за сутки", LogType.DETAILED);
				if (Configuration.CheckMonthValues) LogEvent("Чтение значений за месяц", LogType.DETAILED);
			}

			if (!needCurrent && !needDay)
			{
				IsExchangeRunning = false;
				lock (Fields)
				{
					Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
					Fields["Time"].Quality = 192;
				}
				UpdateEvent();
				return;
			}

			TcpClient client;
			NetworkStream stream;

			// получение подключения
			try
			{
				client = new TcpClient();
				client.Connect(Configuration.Ip, Configuration.Port);

				stream = client.GetStream();

				if (!client.Connected || stream == null) throw new Exception("Не удалось создать подключение");
			}
			catch (Exception e)
			{
				LogEvent("TCP: " + e.Message, LogType.ERROR);
				IsExchangeRunning = false;
				return;
			}

			// настройка соединения
			try
			{
				byte[] b;

				if (needCurrent)
				{
					// все мгновенные значения
					b = ReadAndWrite(new byte[] { 0x00, 0x04, 0x2E, 0x00, 0x00, 0x01, 0x33, 0x39 }, 30);
					Value("Current.P", BitConverter.ToSingle(new byte[] { b[4], b[5], b[6], b[7], }, 0));
					Value("Current.Q", BitConverter.ToSingle(new byte[] { b[8], b[9], b[10], b[11], }, 0));
					Value("Current.U", BitConverter.ToSingle(new byte[] { b[12], b[13], b[14], b[15], }, 0));
					Value("Current.I", BitConverter.ToSingle(new byte[] { b[16], b[17], b[18], b[19], }, 0));
					Value("Current.kP", BitConverter.ToSingle(new byte[] { b[20], b[21], b[22], b[23], }, 0));
					Value("Current.F", BitConverter.ToSingle(new byte[] { b[24], b[25], b[26], b[27], }, 0));
				}

				if (needDay && Configuration.CheckDayValues)
				{
					// накопленная энергия за последние сутки
					b = ReadAndWrite(new byte[] { 0x00, 0x03, 0x2A, 0x00, 0x00, 0x00, 0x03, 0x4C }, 22);
					Value("LastDay.E", 7.5 / 10000 * BitConverter.ToInt32(new byte[] { b[4], b[5], b[6], b[7] }, 0));
				}

				if (needDay && Configuration.CheckMonthValues)
				{
					// накопленная энергия за последний месяц
					b = ReadAndWrite(new byte[] { 0x00, 0x03, 0x2B, 0x00, 0x00, 0x00, 0x03, 0x4C }, 22);
					Value("LastMonth.E", 7.5 / 10000 * BitConverter.ToInt32(new byte[] { b[4], b[5], b[6], b[7] }, 0));
				}

				// идентификационный номер
				//ReadAndWrite(new byte[] { 0x00, 0x03, 0x00, 0x00, 0x00, 0x00, 0x1B, 0x44 }, 8);

				// текущее значение даты и времени
				//ReadAndWrite(new byte[] { 0x00, 0x03, 0x22, 0x00, 0x00, 0x00, 0x63, 0x4E }, 24);

				// архив событий состояния прибора
				//ReadAndWrite(new byte[] { 0x00, 0x03, 0x0F, 0x00, 0x00, 0x00, 0x0F, 0x47 }, 15);

				// архив событий состояния фаз
				//ReadAndWrite(new byte[] { 0x00, 0x03, 0x0E, 0x00, 0x00, 0x00, 0xF3, 0x46 }, 15);

				// квадрант, тариф, сезон и ресурс батареи
				//ReadAndWrite(new byte[] { 0x00, 0x03, 0x21, 0x00, 0x00, 0x00, 0x27, 0x4E }, 10);
			}
			catch (Exception e)
			{
				LogEvent("Ошибка при получении данных: " + e.Message, LogType.ERROR);
				lock (Fields)
				{
					foreach (var field in Fields) field.Value.Quality = 0;
				}
			}
			finally
			{
				IsExchangeRunning = false;

				try { stream.Close(); } catch { }
				try { client.Close(); } catch { }

				lock (Fields)
				{
					Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
					Fields["Time"].Quality = 192;
				}
				UpdateEvent();
			}

			byte[] ReadAndWrite(byte[] command, int size)
			{
				stream.Write(command, 0, command.Length);
				LogEvent("TX: " + Helpers.BytesToString(command), LogType.DETAILED);

				Task.Delay(Configuration.PacketTimeout).Wait();

				byte[] buffer = new byte[size];
				stream.Read(buffer, 0, buffer.Length);
				LogEvent("RX: " + Helpers.BytesToString(buffer), LogType.DETAILED);

				return buffer;
			}

			void Value(string name, object value)
			{
				lock (Fields)
				{
					if (Fields.ContainsKey(name))
					{
						Fields[name].Quality = 192;
						Fields[name].Value = value;
					}
					else
					{
						Fields.Add(name, new DefField { Name = name, Quality = 192, Value = value });
					}
				}
			}
		}
	}
}