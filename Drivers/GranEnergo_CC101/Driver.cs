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
		public event WinLogEvent WinLogEvent;

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
				Fields.Add("Time", new DefField { Name = "Time", Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 });
			}
			catch
			{
				LogEvent("Конфигурация не прочитана из json", LogType.ERROR);
				return false;
			}

			try
			{
				ExchangeTimer = new Timer(Configuration.ExchangeIntervalMs);
				ExchangeTimer.Elapsed += (s, e) => { Exchange(); };
			}
			catch
			{
				LogEvent("Конфигурация не прочитана из json", LogType.ERROR);
				return false;
			}


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

		void Exchange()
		{
			if (!IsDriverActive) return;
			if (IsExchangeRunning) return;

			IsExchangeRunning = true;

			TcpClient client;
			NetworkStream stream;

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

			// получение мгновенных
			try
			{
				byte[] command = new byte[] { 0x00, 0x04, 0x2E, 0x00, 0x00, 0x01, 0x33, 0x39 };
				stream.Write(command, 0, command.Length);
				LogEvent("TX: " + Helpers.BytesToString(command), LogType.DETAILED);

				byte[] buffer = new byte[30];
				stream.Read(buffer, 0, buffer.Length);
				LogEvent("RX: " + Helpers.BytesToString(buffer), LogType.DETAILED);

				Value("Current.P",  BitConverter.ToSingle(new byte[] { buffer[4], buffer[5], buffer[6], buffer[7], }, 0));
				Value("Current.Q",  BitConverter.ToSingle(new byte[] { buffer[8], buffer[9], buffer[10], buffer[11], }, 0));
				Value("Current.U",  BitConverter.ToSingle(new byte[] { buffer[12], buffer[13], buffer[14], buffer[15], }, 0));
				Value("Current.I",  BitConverter.ToSingle(new byte[] { buffer[16], buffer[17], buffer[18], buffer[19], }, 0));
				Value("Current.kP", BitConverter.ToSingle(new byte[] { buffer[20], buffer[21], buffer[22], buffer[23], }, 0));
				Value("Current.F",  BitConverter.ToSingle(new byte[] { buffer[24], buffer[25], buffer[26], buffer[27], }, 0));

				Value("Time", DateTime.Now.ToString("HH:mm:ss"));
				UpdateEvent();
			}
			catch (Exception e)
			{
				LogEvent("Текущие значения: " + e.Message, LogType.ERROR);
				IsExchangeRunning = false;
				return;
			}

			// получение часовых
			try
			{

			}
			catch (Exception e)
			{
				LogEvent("Часовые значения: " + e.Message, LogType.ERROR);
				IsExchangeRunning = false;
				return;
			}

			// получение суточных
			try
			{

			}
			catch (Exception e)
			{
				LogEvent("Суточные значения: " + e.Message, LogType.ERROR);
				IsExchangeRunning = false;
				return;
			}

			try
			{
				stream.Close();
				client.Close();
			} catch { }

			IsExchangeRunning = false;
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
