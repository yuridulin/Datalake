using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace SmartMetering
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

			Receiver = new Timer(Configuration.ExchangeDelaySeconds * 1000);
			Receiver.Elapsed += (s, e) => Receive();

			try
			{
				IsActive = true;

				Receiver.Start();

				LogEvent("Мониторинг запущен");

				Task.Run(Receive);

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
				Receiver.Stop();
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

		Stream Stream { get; set; }

		Timer Receiver { get; set; }

		bool IsActive { get; set; } = false;

		byte[] RX { get; set; }

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
					Fields[ name ].Quality = 0;
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
					Fields[ name ].Value = value;
					Fields[ name ].Quality = quality;
				}
				else
				{
					Fields.Add(name, new DefField { Value = value, Quality = quality });
				}
			}

			UpdateEvent();
		}

		void Receive()
		{
			if (!IsActive) return;

			try
			{
				var client = new TcpClient();
				client.Connect(Configuration.Endpoint, Configuration.Port);

				Stream = client.GetStream();

				// Поиск в сети
				//Get("06 00 03");

				// Текущее время
				Get("04 00 01", new string[] { "Now.DateTime" }, 0);

				// Текущее накопление энергии - общая
				//Get("0A 00 01 02 00", new string[] { "Power.Current.Value" }, 0.0001);

				// Полная мощность (группа)
				//Get("0A 00 0D 1E", new string[] { "Now.Power.Full.Summ", "Now.Power.Full.A", "Now.Power.Full.B", "Now.Power.Full.C" }, 0.0001);

				// Активная мощность (группа)
				//Get("0A 00 0E 1E", new string[] { "Now.Power.Active.Summ", "Now.Power.Active.A", "Now.Power.Active.B", "Now.Power.Active.C" }, 0.0001);

				// Реактивная мощность (группа)
				//Get("0A 00 10 1E", new string[] { "Now.Power.Reactive.Summ", "Now.Power.Reactive.A", "Now.Power.Reactive.B", "Now.Power.Reactive.C" }, 0.0001);

				// Ток (группа)
				//Get("0A 00 16 1C", new string[] { "Now.Amperage.Summ", "Now.Amperage.A", "Now.Amperage.B", "Now.Amperage.C" }, 0.01);

				// Напряжение (группа)
				//Get("0A 00 18 1C", new string[] { "Now.Voltage.Summ", "Now.Voltage.A", "Now.Voltage.B", "Now.Voltage.C" }, 0.01);

				// Частота (группа)
				//Get("0A 00 1A 1C", new string[] { "Now.Freq.Summ", "Now.Freq.A", "Now.Freq.B", "Now.Freq.C" }, 0.01);

				// Большой запрос
				Get("0A 00 0D 1E 0E 1E 10 1E 16 1C 18 1C 1A 1C", new string[] 
				{ 
					"Now.Power.Full.Summ", "Now.Power.Full.A", "Now.Power.Full.B", "Now.Power.Full.C",
					null, "Now.Power.Active.Summ", "Now.Power.Active.A", "Now.Power.Active.B", "Now.Power.Active.C",
					null, "Now.Power.Reactive.Summ", "Now.Power.Reactive.A", "Now.Power.Reactive.B", "Now.Power.Reactive.C",
					null, "Now.Amperage.A", "Now.Amperage.B", "Now.Amperage.C",
					null, "Now.Voltage.A", "Now.Voltage.B", "Now.Voltage.C",
					null, "Now.Freq.A", "Now.Freq.B", "Now.Freq.C",
				}, 0.01);

				// Энергия за прошлый день - сумма
				Get("0B 00 05 02 00 01 01", new string[] { "Day.Power.Full.Summ" }, 0.00001);

				// Энергия за прошлый месяц - сумма
				Get("0B 00 09 02 00 01 01", new string[] { "Month.Power.Full.Summ" }, 0.0000001);

				client.Close();
			}
			catch (ArgumentNullException e)
			{
				LogEvent("Ошибка ArgumentNullException в задаче Receive: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
			}
			catch (ArgumentOutOfRangeException e)
			{
				LogEvent("Ошибка ArgumentOutOfRangeException в задаче Receive: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
			}
			catch (IOException e)
			{
				LogEvent("Ошибка IOException в задаче Receive: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
			}
			catch (ObjectDisposedException e)
			{
				LogEvent("Ошибка ObjectDisposedException в задаче Receive: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
			}
			catch (SocketException e)
			{
				if (IsActive) LogEvent("Ошибка SocketException в задаче Receive: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
			}
			catch (InvalidOperationException e)
			{
				LogEvent("Ошибка InvalidOperationException в задаче Receive: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
			}
			catch (AggregateException e)
			{
				LogEvent("Ошибка AggregateException в задаче Receive: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
			}
		}

		void Get(string command)
		{
			int read = 0;

			byte[] tx = Helpers.StringToBytes("06 00 00 00 00 00 06 " + command);
			tx = Helpers.StringToBytes("C0 " + Helpers.BytesToString(tx) + " " + Helpers.BytesToString(CRC(tx, tx.Length)) + " C0");
			Stream.Write(tx, 0, tx.Length);
			LogEvent("TX: " + Helpers.BytesToString(tx).Replace("C0", "").Trim(), LogType.DETAILED);

			byte[] rx = new byte[ 50 ];
			Task.Run(() => { read = Stream.Read(rx, 0, rx.Length); }).Wait(TimeSpan.FromSeconds(2));

			string answer = Helpers.BytesToString(rx);

			if (answer.LastIndexOf("C0") < 4)
			{
				LogEvent("RX: " + answer, LogType.DETAILED);
				throw new Exception("Нет ответа");
			}
			else
			{
				answer = answer
					.Substring(0, answer.LastIndexOf("C0"))
					.Replace("C0", "")
					.Replace("DB DC", "C0")
					.Replace("DB DD", "DB")
					.Trim();
				LogEvent("RX: " + answer, LogType.DETAILED);
				RX = Helpers.StringToBytes(answer);
			}
		}

		void Get(string command, string[] names, double delimeter = 1)
		{
			try
			{
				Get(command);
			}
			catch (Exception e)
			{
				LogEvent(e.Message, LogType.ERROR);
				foreach (var name in names)
				{
					SetBadQuality(name);
				}
				Stream.Flush();
				return;
			}

			var values = GetCEValue(RX, 10);
			LogEvent("Ожидается/получено: " + names.Length + "/" + values.Length, LogType.DETAILED);
			for (byte i = 0; i < Math.Min(values.Length, names.Length); i++)
			{
				if (names[ i ] == null) continue;
				if (delimeter == 0)
				{
					SetValue(names[ i ], new DateTime(2012, 1, 1).AddSeconds(values[ i ]).ToString("dd.MM.yyyy HH:mm:ss"));
				}
				else
				{
					SetValue(names[ i ], Math.Round(values[ i ] * delimeter, 4));
				}
			}
		}

		uint[] GetCEValue(byte[] buf, int startIndex)
		{
			var values = new List<uint>();
			
			do
			{
				int i = 0; 
				uint val = 0;

				do
				{
					// байт кладется на маску 0111 1111 = 0x7F, это биты данных
					// первый бит - флаг, показывающий, к какому значению относится текущий байт
					// если первый бит 1, то следующий байт относится к этому же числу, и нужно его дочитывать
					val |= (uint)((buf[ startIndex ] & 0x7f) << (7 * i));
					LogEvent("Байт: " + i + " значение " + val, LogType.DETAILED);
					i++;
				}
				// бит кладется на маску 1000 0000 = 0x80, чтобы получить первый бит и понять, стоит ли брать следующий байт
				while ((buf[ startIndex++ ] & 0x80) != 0);

				values.Add(val);
				LogEvent("Значение: " + val, LogType.DETAILED);
			}
			while (startIndex < buf.Length - 2);

			return values.ToArray();
		}

		byte[] CRC(byte[] buff, int size)
		{
			byte i;
			ushort j = 0;
			ushort crc = 0;
			while (size-- > 0)
			{
				crc ^= (ushort)(((ushort)buff[ j++ ]) << 8);
				for (i = 0; i < 8; i++)
				{
					crc = ((crc & 0x8000) != 0) ? (ushort)((crc << 1) ^ 0x8005) : (ushort)(crc << 1);
				}
			}
			return new byte[] { (byte)(crc >> 8), (byte)(crc & 0xff) };
		}
	}
}
