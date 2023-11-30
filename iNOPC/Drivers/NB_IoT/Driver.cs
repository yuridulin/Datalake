using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

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

			SetBadQuality();

			BadQualityChecker = new Timer(1000);
			BadQualityChecker.Elapsed += BadQualityChecker_Elapsed;

			LastUpdateTime = DateTime.MinValue;

			try
			{
				IsActive = true;

				Task.Run(Listen);
				BadQualityChecker.Start();

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

			try
			{
				Listener.Stop();
				BadQualityChecker.Stop();
			}
			catch { }

			SetBadQuality();

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			throw new NotImplementedException();
		}

		// Реализация получения данных

		Configuration Configuration { get; set; }

		TcpListener Listener { get; set; }

		Timer BadQualityChecker { get; set; }

		bool IsActive { get; set; } = false;

		bool Err(string text)
		{
			LogEvent(text, LogType.ERROR);
			return false;
		}

		void SetBadQuality(string name = null)
		{
			lock (Fields)
			{
				if (string.IsNullOrEmpty(name))
				{
					foreach (var v in Fields.Values)
					{
						v.Quality = 0;
					}
				}
				else if (Fields.ContainsKey(name))
				{
					Fields[name].Quality = 0;
				}
			}

			SetValue("Time", DateTime.Now.ToString("HH:mm:ss"));
		}

		void SetValue(string name, object value, ushort quality = 192)
		{
			lock (Fields)
			{
				if (Fields.ContainsKey(name))
				{
					Fields[name].Value = value;
					Fields[name].Quality = quality;
				}
				else
				{
					Fields.Add(name, new DefField { Value = value, Quality = quality });
				}
			}

			UpdateEvent();
		}

		void BadQualityChecker_Elapsed(object sender, ElapsedEventArgs e)
		{
			if (LastUpdateTime == DateTime.MinValue) return;

			if ((DateTime.Now - LastUpdateTime).TotalMinutes >= Configuration.MinutesForGoodValues)
			{
				SetBadQuality(); 
				LastUpdateTime = DateTime.MinValue;
				LogEvent("Данные признаны недостоверными по таймауту", LogType.WARNING);
			}
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

					byte[] bytes = new byte[1024];
					int count = 0;

					bool isReadingDone = Task
						.Run(() => 
						{
							count = stream.Read(bytes, 0, 1024);
						})
						.Wait(TimeSpan.FromSeconds(3));

					if (!isReadingDone)
					{
						LogEvent("Чтение потока прервано по таймауту");
					}

					bytes = bytes.Take(count).ToArray();
					LogEvent("Получено: " + Helpers.BytesToString(bytes), LogType.DETAILED);
					ParseValue(bytes);

					bytes = Encoding.UTF8.GetBytes("<!OK>");
					stream.Write(bytes, 0, bytes.Length);

					stream.Close();
					client.Close();

					LogEvent("Клиент отключён");
				}
				catch (ArgumentNullException e)
				{
					LogEvent("Ошибка ArgumentNullException в задаче Listen: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
				}
				catch (ArgumentOutOfRangeException e)
				{
					LogEvent("Ошибка ArgumentOutOfRangeException в задаче Listen: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
				}
				catch (IOException e)
				{
					LogEvent("Ошибка IOException в задаче Listen: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
				}
				catch (ObjectDisposedException e)
				{
					LogEvent("Ошибка ObjectDisposedException в задаче Listen: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
				}
				catch (SocketException e)
				{
					if (IsActive) LogEvent("Ошибка SocketException в задаче Listen: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
				}
				catch (InvalidOperationException e)
				{
					LogEvent("Ошибка InvalidOperationException в задаче Listen: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
				}
				catch (AggregateException e)
				{
					LogEvent("Ошибка AggregateException в задаче Listen: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
				}
			}
		}

		void ParseValue(byte[] packet)
		{
			try
			{
				var json = JsonConvert.DeserializeObject<Answer>(Encoding.UTF8.GetString(packet));

				SetValue("Message.DeviceType", json.Message.dev);
				SetValue("Message.DeviceIMEI", json.Message.imei);
				SetValue("Message.PacketNum", json.Message.num);

				SetValue("Telemetry.Date", new DateTime(1970, 1, 1).AddSeconds(json.Telemetry.date).ToString("dd.MM.yyyy HH:mm:ss"));
				SetValue("Telemetry.RSSI", json.Telemetry.rssi);
				SetValue("Telemetry.BatteryCharge", json.Telemetry.bat_mv);
				SetValue("Telemetry.Temperature", json.Telemetry.temp);
				SetValue("Telemetry.ResistConductorA", json.Telemetry.raw);
				SetValue("Telemetry.ResistInsulationA", json.Telemetry.rai);
				SetValue("Telemetry.ResistConductorC", json.Telemetry.rbw);
				SetValue("Telemetry.ResistInsulationB", json.Telemetry.rbi);
				SetValue("Telemetry.ResistConductorC", json.Telemetry.rcw);
				SetValue("Telemetry.ResistInsulationC", json.Telemetry.rci);
				SetValue("Telemetry.ResistConductorD", json.Telemetry.rdw);
				SetValue("Telemetry.ResistInsulationD", json.Telemetry.rdi);
				SetValue("Telemetry.Input1", json.Telemetry.di1);
				SetValue("Telemetry.Input2", json.Telemetry.di2);
			}
			catch (Exception e)
			{
				LogEvent("JSON не прочитан: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
			}

			LastUpdateTime = DateTime.Now;
		}

		DateTime LastUpdateTime { get; set; }
	}
}
