using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace Energomera_CE102
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

				var minTick = new[] { 2, Configuration.DateTimeInterval, Configuration.CurrentInterval, Configuration.DailyInterval, Configuration.MonthlyInterval }.Min() * 60000;

				ExchangeTimer = new Timer(minTick);
				ExchangeTimer.Elapsed += (s, e) => { Exchange(); };

				Timeouts = new DateTime[] { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, };

				DeviceNumber = BitConverter.GetBytes(Configuration.DeviceNumber);
				StationNumber = BitConverter.GetBytes(Configuration.StationNumber);
				Password = BitConverter.GetBytes(Configuration.Password);
			}
			catch (Exception e)
			{
				LogEvent("Не удалось применить конфигурацию: " + e.Message, LogType.ERROR);
				return false;
			}

			IsDriverActive = true;
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

		// реализация протокола получения данных

		Configuration Configuration { get; set; }

		Timer ExchangeTimer { get; set; }

		bool IsDriverActive { get; set; }

		byte[] DeviceNumber { get; set; }

		byte[] StationNumber { get; set; }

		byte[] Password { get; set; }

		DateTime[] Timeouts { get; set; }

		TcpClient Client { get; set; }

		NetworkStream Stream { get; set; }

		void Exchange()
		{
			LogEvent("Очередной тик таймера", LogType.DETAILED);

			if (!IsDriverActive) return;

			try { Stream?.Close(); Stream = null; } catch { }
			try { Client?.Close(); Client = null; } catch { }

			//IsExchangeRunning = true;

			// Расчёт необходимости получения данных за этот тик
			DateTime now = DateTime.Now;
			bool[] reasons = new bool[]
			{
				Configuration.CheckDateTime && Math.Round((now - Timeouts[0]).TotalMinutes) >= Configuration.DateTimeInterval,
				Configuration.CheckCurrentData && Math.Round((now - Timeouts[1]).TotalMinutes) >= Configuration.CurrentInterval,
				Configuration.CheckDailyData && Math.Round((now - Timeouts[2]).TotalMinutes) >= Configuration.DailyInterval,
				Configuration.CheckMonthlyData && Math.Round((now - Timeouts[3]).TotalMinutes) >= Configuration.MonthlyInterval,
			};

			for (int i = 0; i < reasons.Length; i++)
			{
				LogEvent("Флаг [" + i + "] = " + (now - Timeouts[i]).TotalMinutes + " | " + (reasons[i] ? "yes" : "no"), LogType.DETAILED);
			}

			if (!reasons[0] && !reasons[1] && !reasons[2] && !reasons[3] && !reasons[4])
			{
				LogEvent("Нет необходимости в опросе", LogType.DETAILED);
				return;
			}

			// получение подключения
			try
			{
				Client = new TcpClient();
				Client.Connect(Configuration.Ip, Configuration.Port);

				Stream = Client.GetStream();

				if (!Client.Connected || Stream == null) throw new Exception("Не удалось создать подключение");
			}
			catch (Exception e)
			{
				LogEvent("TCP: " + e.Message, LogType.ERROR);
				return;
			}

			if (!IsDriverActive) return;

			// задержка после подключения
			Task.Delay(Configuration.ExchangeDelay).Wait();
			if (!IsDriverActive) return;

			// получение данных
			try
			{
				byte[] b;

				// дата и время
				if (reasons[0])
				{
					LogEvent("Получение даты и времени", LogType.DETAILED);
					if (ReadAndWrite(Command48(new byte[] { 0xD0, 0x01, 0x20 }), 18, out b))
					{
						Value("DeviceTime", new DateTime(2000 + Hex(b[15]), Hex(b[14]), Hex(b[13]), Hex(b[11]), Hex(b[10]), Hex(b[9])).ToString("dd.MM.yyyy HH:mm:ss"));
						Timeouts[0] = now;
					}
					else if (Configuration.SetBadQuality)
					{
						Value("DeviceTime", "", 0);
					}
				}

				// текущее значение
				if (reasons[1])
				{
					LogEvent("Получение текущего значения энергии", LogType.DETAILED);
					if (ReadAndWrite(Command48(new byte[] { 0xD1, 0x01, 0x31, 0x00 }), 15, out b))
					{
						Value("CurrentEnergy", BitConverter.ToInt32(new byte[] { b[9], b[10], b[11], b[12] }, 0) * 0.01);
						Timeouts[1] = now;
					}
					else if (Configuration.SetBadQuality)
					{
						Value("CurrentEnergy", 0, 0);
					}
				}

				// энергия за сутки
				if (reasons[2])
				{
					LogEvent("Получение значения энергии на конец суток", LogType.DETAILED);
					if (ReadAndWrite(Command48(new byte[] { 0xD1, 0x01, 0x2F, 0x01 }), 15, out b))
					{
						Value("EnergyLastDay", BitConverter.ToInt32(new byte[] { b[9], b[10], b[11], b[12] }, 0) * 0.01);
						Timeouts[2] = now;
					}
					else if (Configuration.SetBadQuality)
					{
						Value("EnergyLastDay", 0, 0);
					}
				}
				
				// энергия за месяц
				if (reasons[3])
				{
					LogEvent("Получение значения энергии на конец месяца", LogType.DETAILED);
					if (ReadAndWrite(Command48(new byte[] { 0xD1, 0x01, 0x31, 0x01 }), 15, out b))
					{
						Value("EnergyLastMonth", BitConverter.ToInt32(new byte[] { b[9], b[10], b[11], b[12] }, 0) * 0.01);
						Timeouts[3] = now;
					}
					else if (Configuration.SetBadQuality)
					{
						Value("EnergyLastMonth", 0, 0);
					}
				}
			}
			catch (Exception e)
			{
				LogEvent("Ошибка при получении данных: " + e.Message, LogType.ERROR);
				if (Configuration.SetBadQuality) BadQuality();
			}
			finally
			{
				try { Stream?.Close(); Stream = null; } catch { }
				try { Client?.Close(); Client = null; } catch { }

				Value("Time", DateTime.Now.ToString("HH:mm:ss"));
				if (IsDriverActive) UpdateEvent();
			}

			
			void Value(string name, object value, ushort quality = 192)
			{
				lock (Fields)
				{
					if (Fields.ContainsKey(name))
					{
						Fields[name].Quality = quality;
						Fields[name].Value = value;
					}
					else
					{
						Fields.Add(name, new DefField { Name = name, Quality = quality, Value = value });
					}
				}
			}

			void BadQuality()
			{
				lock (Fields)
				{
					foreach (var field in Fields) if (field.Key != "Time") field.Value.Quality = 0;
				}
			}
			
			bool ReadAndWrite(byte[] command, int size, out byte[] answer)
			{
				Stream.Write(command, 0, command.Length);
				LogEvent("TX: " + Helpers.BytesToString(command), LogType.REGULAR);

				Task.Delay(Configuration.PacketTimeout).Wait();

				byte[] buffer = new byte[size];
				Stream.Read(buffer, 0, buffer.Length);
				LogEvent("RX: " + Helpers.BytesToString(buffer), LogType.REGULAR);

				answer = buffer;
				return buffer[0] == 0xC0 && buffer[size - 1] == 0xC0;
			}

			byte[] Command48(byte[] command)
			{
				byte[] polynom_0xB5 = new byte[]
				{
					0x00, 0xb5, 0xdf, 0x6a, 0x0b, 0xbe, 0xd4, 0x61, 0x16, 0xa3, 0xc9, 0x7c, 0x1d, 0xa8, 0xc2, 0x77,
					0x2c, 0x99, 0xf3, 0x46, 0x27, 0x92, 0xf8, 0x4d, 0x3a, 0x8f, 0xe5, 0x50, 0x31, 0x84, 0xee, 0x5b,
					0x58, 0xed, 0x87, 0x32, 0x53, 0xe6, 0x8c, 0x39, 0x4e, 0xfb, 0x91, 0x24, 0x45, 0xf0, 0x9a, 0x2f,
					0x74, 0xc1, 0xab, 0x1e, 0x7f, 0xca, 0xa0, 0x15, 0x62, 0xd7, 0xbd, 0x08, 0x69, 0xdc, 0xb6, 0x03,
					0xb0, 0x05, 0x6f, 0xda, 0xbb, 0x0e, 0x64, 0xd1, 0xa6, 0x13, 0x79, 0xcc, 0xad, 0x18, 0x72, 0xc7,
					0x9c, 0x29, 0x43, 0xf6, 0x97, 0x22, 0x48, 0xfd, 0x8a, 0x3f, 0x55, 0xe0, 0x81, 0x34, 0x5e, 0xeb,
					0xe8, 0x5d, 0x37, 0x82, 0xe3, 0x56, 0x3c, 0x89, 0xfe, 0x4b, 0x21, 0x94, 0xf5, 0x40, 0x2a, 0x9f,
					0xc4, 0x71, 0x1b, 0xae, 0xcf, 0x7a, 0x10, 0xa5, 0xd2, 0x67, 0x0d, 0xb8, 0xd9, 0x6c, 0x06, 0xb3,
					0xd5, 0x60, 0x0a, 0xbf, 0xde, 0x6b, 0x01, 0xb4, 0xc3, 0x76, 0x1c, 0xa9, 0xc8, 0x7d, 0x17, 0xa2,
					0xf9, 0x4c, 0x26, 0x93, 0xf2, 0x47, 0x2d, 0x98, 0xef, 0x5a, 0x30, 0x85, 0xe4, 0x51, 0x3b, 0x8e,
					0x8d, 0x38, 0x52, 0xe7, 0x86, 0x33, 0x59, 0xec, 0x9b, 0x2e, 0x44, 0xf1, 0x90, 0x25, 0x4f, 0xfa,
					0xa1, 0x14, 0x7e, 0xcb, 0xaa, 0x1f, 0x75, 0xc0, 0xb7, 0x02, 0x68, 0xdd, 0xbc, 0x09, 0x63, 0xd6,
					0x65, 0xd0, 0xba, 0x0f, 0x6e, 0xdb, 0xb1, 0x04, 0x73, 0xc6, 0xac, 0x19, 0x78, 0xcd, 0xa7, 0x12,
					0x49, 0xfc, 0x96, 0x23, 0x42, 0xf7, 0x9d, 0x28, 0x5f, 0xea, 0x80, 0x35, 0x54, 0xe1, 0x8b, 0x3e,
					0x3d, 0x88, 0xe2, 0x57, 0x36, 0x83, 0xe9, 0x5c, 0x2b, 0x9e, 0xf4, 0x41, 0x20, 0x95, 0xff, 0x4a,
					0x11, 0xa4, 0xce, 0x7b, 0x1a, 0xaf, 0xc5, 0x70, 0x07, 0xb2, 0xd8, 0x6d, 0x0c, 0xb9, 0xd3, 0x66
				};

				byte[] body = new byte[8 + command.Length];
				body[0] = DeviceNumber[0];
				body[1] = DeviceNumber[1];
				body[2] = StationNumber[0];
				body[3] = StationNumber[1];
				body[4] = Password[0];
				body[5] = Password[1];
				body[6] = Password[2];
				body[7] = Password[3];
				for (int i = 0; i < command.Length; i++) body[8 + i] = command[i];

				byte[] tx = new byte[2 + body.Length + 2];
				tx[0] = 0xC0;
				tx[1] = 0x48;
				for (int i = 0; i < body.Length; i++)
				{
					tx[2 + i] = body[i];
				}
				tx[tx.Length - 1] = 0xC0;

				byte crc = 0xA6; // Экспериментально найденное значение
				for (int i = 0; i < body.Length; i++)
				{
					crc = polynom_0xB5[crc ^ body[i]];
				}
				tx[tx.Length - 2] = crc;

				return tx;
			}

			int Hex(byte b)
			{
				return int.TryParse(b.ToString("X2"), out int i) ? i : 0;
			}
		}
	}
}