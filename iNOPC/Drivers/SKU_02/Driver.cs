using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Drivers.SKU_02
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

		List<Field> PreFields { get; set; } = new List<Field> 
		{
			new Field { Name = "E1", Start = 30, IsFloat = false, Demiliter = 1000 },
			new Field { Name = "E2", Start = 34, IsFloat = false, Demiliter = 1000 },
			new Field { Name = "V1", Start = 38, IsFloat = false, Demiliter = 100 },
			new Field { Name = "V2", Start = 42, IsFloat = false, Demiliter = 100 },
			new Field { Name = "A", Start = 46, IsFloat = false, Demiliter = 3600 },
			new Field { Name = "E", Start = 50, IsFloat = false, Demiliter = 1000 },
			new Field { Name = "V", Start = 54, IsFloat = false, Demiliter = 100 },
			new Field { Name = "P", Start = 58, IsFloat = true },
			new Field { Name = "P1", Start = 62, IsFloat = true },
			new Field { Name = "P2", Start = 66, IsFloat = true },
			new Field { Name = "F1", Start = 70, IsFloat = true },
			new Field { Name = "F2", Start = 74, IsFloat = true },
			new Field { Name = "dT1", Start = 78, IsFloat = true },
			new Field { Name = "dT2", Start = 82, IsFloat = true },
			new Field { Name = "T1", Start = 86, IsFloat = true },
			new Field { Name = "T2", Start = 90, IsFloat = true },
			new Field { Name = "T3", Start = 94, IsFloat = true },
		};

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

				Get();

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

		void Get()
		{
			if (!IsActive) throw new Exception("Опрос прерван");

			byte[] tx = Helpers.StringToBytes("68 00 22 68 00 02 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 09 2C 37 00 00 00 00 20 12 5B A4 16");

			ByteExchange(tx);
			if (RX.Length == 0) return;

			if (!IsActive) throw new Exception("Опрос прерван");

			float value;

			foreach (var field in PreFields)
			{

				if (field.IsFloat)
				{
					value = BitConverter.ToSingle(new byte[] {
						RX[field.Start + 3],
						RX[field.Start + 2],
						RX[field.Start + 1],
						RX[field.Start + 0]
					}, 0);
				}
				else
				{
					value = Convert.ToSingle(BitConverter.ToUInt32(new byte[] {
						RX[field.Start + 3],
						RX[field.Start + 2],
						RX[field.Start + 1],
						RX[field.Start + 0]
					}, 0));
				}

				SetValue(field.Name, Math.Round(value / field.Demiliter, 4));
			}
		}

		void ByteExchange(byte[] tx)
		{
			try
			{
				int read = 0;

				Stream.Write(tx, 0, tx.Length);
				LogEvent("TX: " + Helpers.BytesToString(tx), LogType.DETAILED);

				byte[] rx = new byte[130];
				Task.Run(() => { read = Stream.Read(rx, 0, rx.Length); }).Wait(TimeSpan.FromSeconds(Configuration.RxTimeoutSeconds));

				if (read == 0)
				{
					LogEvent("RX: " + Helpers.BytesToString(rx), LogType.DETAILED);
					throw new Exception("Нет ответа");
				}
				else
				{
					RX = rx.Take(read).ToArray();

					if (RX[0] != 0x68 || RX[read - 1] != 0x16)
					{
						throw new Exception("Данные не достоверны");
					}

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
	}
}
