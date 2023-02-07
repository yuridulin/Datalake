using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Drivers.TEM_104
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
			SetValue("Time", DateTime.Now.ToString("HH:mm:ss"), 192);
			UpdateEvent();

			Worker = new Timer(Configuration.ExchangeDelaySeconds * 1000);
			Worker.Elapsed += (s, e) => Work();

			try
			{
				IsActive = true;

				Worker.Start();

				LogEvent("Мониторинг запущен");

				Task.Run(Work);

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
				Worker.Stop();
			}
			catch { }

			SetBadQuality(true);
			UpdateEvent();

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			throw new NotImplementedException();
		}

		// Реализация получения данных

		Configuration Configuration { get; set; }

		Stream Stream { get; set; }

		Timer Worker { get; set; }

		bool IsActive { get; set; } = false;

		byte[] RX { get; set; }

		bool Err(string text)
		{
			LogEvent(text, LogType.ERROR);
			return false;
		}

		void SetBadQuality(bool withTime = false)
		{
			lock (Fields)
			{
				foreach (var v in Fields)
				{
					if (withTime || v.Key != "Time")
					{
						v.Value.Quality = 0;
					}
				}
			}
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
		}

		void Work()
		{
			if (!IsActive) return;

			try
			{
				var client = new TcpClient();
				client.Connect(Configuration.Endpoint, Configuration.Port);

				Stream = client.GetStream();

				if (Establish())
				{
					Get("22 00 08", new [] { "Temp1", "Temp2" });
					Get("22 10 08", new [] { "Pressure1", "Pressure2" });
					Get("22 20 08", new [] { "Ro1", "Ro2" });
					Get("22 30 08", new [] { "Hent1", "Hent2" });
					Get("22 40 08", new [] { "Rshv1", "Rshv2" });
					Get("22 50 08", new [] { "Rshm1", "Rshm12" });
					Get("22 60 08", new [] { "Pwr1", "Pwr2" });
				}

				client.Close();

				SetValue("Time", DateTime.Now.ToString("HH:mm:ss"), 192);
				UpdateEvent();
			}
			catch (Exception e)
			{
				LogEvent("Ошибка: " + e.Message, LogType.ERROR);
				SetBadQuality();
				SetValue("Time", DateTime.Now.ToString("HH:mm:ss"), 192);
				UpdateEvent();
			}
		}

		bool Establish()
		{
			byte[] request = new byte[]
			{
				0x55,
				Configuration.DeviceId,
				(byte)(0xFF - Configuration.DeviceId),
				0x00,
				0x00,
				0x00
			};

			byte[] tx = WithCRC(request);

			ByteExchange(tx);

			if (RX.Length == 0) return false;
			if (RX[0] != 0xAA) return false;
			return true;
		}

		void Get(string start, string[] names)
		{
			if (!IsActive) throw new Exception("Опрос прерван");

			byte[] startbytes = Helpers.StringToBytes(start);
			byte[] request = new byte[]
			{
				0x55,
				Configuration.DeviceId,
				(byte)(0xFF - Configuration.DeviceId),
				0x0C,
				0x01,
				0x03,
				startbytes[0],
				startbytes[1],
				startbytes[2]
			};

			byte[] tx = WithCRC(request);

			ByteExchange(tx);
			if (RX.Length == 0) return;

			if (!IsActive) throw new Exception("Опрос прерван");

			for (int i = 0; i < names.Length; i++)
			{
				SetValue(names[i], BitConverter.ToSingle(new[] { 
					RX[6 + i * 4 + 3],
					RX[6 + i * 4 + 2],
					RX[6 + i * 4 + 1],
					RX[6 + i * 4 + 0]
				}, 0));
			}
		}

		void ByteExchange(byte[] tx)
		{
			try
			{
				int read = 0;

				Stream.Write(tx, 0, tx.Length);
				LogEvent("TX: " + Helpers.BytesToString(tx), LogType.DETAILED);

				byte[] rx = new byte[80];
				Task.Run(() => { read = Stream.Read(rx, 0, rx.Length); }).Wait(TimeSpan.FromSeconds(Configuration.RxTimeoutSeconds));

				if (read == 0 || rx[0] != 0xAA)
				{
					LogEvent("RX: " + Helpers.BytesToString(rx), LogType.DETAILED);
					throw new Exception("Нет ответа");
				}
				else
				{
					RX = rx.Take(read).ToArray();
					LogEvent("RX: " + Helpers.BytesToString(RX), LogType.DETAILED);
				}
			}
			catch (Exception e)
			{
				RX = new byte[0];
				LogEvent(e.Message, LogType.ERROR);
				Stream.Flush();
			}
		}

		byte[] WithCRC(byte[] buff)
		{
			byte[] result = new byte[buff.Length + 1];
			byte crc = 0;
			for (int i = 0; i < buff.Length; i++) 
			{ 
				crc += buff[i];
				result[i] = buff[i];
			}

			result[result.Length - 1] = (byte)(0xFF - crc);
			return result;
		}
	}
}
