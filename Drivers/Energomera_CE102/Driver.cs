using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

				ExchangeTimer = new Timer(5000);
				ExchangeTimer.Elapsed += (s, e) => { Exchange(); };

				Timeouts = new DateTime[] { DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue, DateTime.MinValue };

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

		// реализация протокола получения данных

		Configuration Configuration { get; set; }

		Timer ExchangeTimer { get; set; }

		bool IsDriverActive { get; set; }

		bool IsExchangeRunning { get; set; }

		byte[] DeviceNumber { get; set; }

		byte[] StationNumber { get; set; }

		byte[] Password { get; set; }

		DateTime[] Timeouts { get; set; }

		void Exchange()
		{
			if (!IsDriverActive) return;
			if (IsExchangeRunning) return;

			IsExchangeRunning = true;

			TcpClient client;
			NetworkStream stream;
			
			// Расчёт необходимости получения данных за этот тик
			DateTime now = DateTime.Now;
			bool[] reasons = new bool[]
			{
				Configuration.CheckDateTime && (now - Timeouts[0]).TotalMinutes > Configuration.DateTimeInterval,
				Configuration.CheckCurrentData && (now - Timeouts[1]).TotalMinutes > Configuration.CurrentInterval,
				Configuration.CheckDailyData && (now - Timeouts[2]).TotalMinutes > Configuration.DailyInterval,
				Configuration.CheckMonthlyData && (now - Timeouts[3]).TotalMinutes > Configuration.MonthlyInterval,
				Configuration.CheckPower && (now - Timeouts[4]).TotalMinutes > Configuration.PowerInterval
			};
			if (!reasons[0] && !reasons[1] && !reasons[2] && !reasons[3] && !reasons[4])
			{
				IsExchangeRunning = false;
				return;
			}

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

			if (!IsDriverActive) return;

			// задержка после подключения
			Task.Delay(Configuration.ExchangeDelay).Wait();
			if (!IsDriverActive) return;

			// получение данных
			try
			{
				byte[] b;

				// поиск и "авторизация"
				//ReadAndWrite(Command48(new byte[] { 0xD0, 0x01, 0x00 }), 17, out b);
				//ReadAndWrite(Command48(new byte[] { 0xD0, 0x01, 0x60 }), 23, out b);
				//ReadAndWrite(Command48(new byte[] { 0xD0, 0x01, 0x1A, 0x00 }), 19, out b);
				//ReadAndWrite(Command48(new byte[] { 0xD0, 0x01, 0x1A, 0x00 }), 19, out b);
				//ReadAndWrite(Command54(new byte[] { 0xD0, 0x04, 0x02 }, new byte[] { 0x00, 0x04, 0x00, 0x04, 0x00 }), 17, out b);

				// дата и время
				if (reasons[0])
				{
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
					if (ReadAndWrite(Command54(new byte[] { 0xD0, 0x05, 0x02 }, new byte[] { 0x00, 0x18, 0x00, 0x00, 0x03, 0x01 }), 20, out b))
					{
						Value("CurrentEnergy", BitConverter.ToInt32(new byte[] { b[13], b[14], b[15], b[16] }, 0) * 0.01);
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
					if (ReadAndWrite(Command54(new byte[] { 0xD0, 0x06, 0x02 }, new byte[] { 0x00, 0x28, 0x00, 0x00, 0x03, 0x01, 0x01 }), 20, out b))
					{
						Value("EnergyLastDay", BitConverter.ToInt32(new byte[] { b[13], b[14], b[15], b[16] }, 0) * 0.01);
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
					if (ReadAndWrite(Command54(new byte[] { 0xD0, 0x05, 0x02 }, new byte[] { 0x00, 0x48, 0x00, 0x00, 0x03, 0x11 }), 20, out b))
					{
						Value("EnergyLastMonth", BitConverter.ToInt32(new byte[] { b[13], b[14], b[15], b[16] }, 0) * 0.01);
						Timeouts[3] = now;
					}
					else if (Configuration.SetBadQuality)
					{
						Value("EnergyLastMonth", 0, 0);
					}
				}

				// энергия за месяц
				if (reasons[4])
				{
					if (ReadAndWrite(Command54(new byte[] { 0xD0, 0x05, 0x02 }, new byte[] { 0x00, 0x48, 0x00, 0x00, 0x03, 0x11 }), 20, out b))
					{
						Value("CurrentPower", BitConverter.ToInt32(new byte[] { b[13], b[14], b[15], b[16] }, 0) * 0.01);
						Timeouts[4] = now;
					}
					else if (Configuration.SetBadQuality)
					{
						Value("CurrentPower", 0, 0);
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
				IsExchangeRunning = false;

				try { stream.Close(); } catch { }
				try { client.Close(); } catch { }

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
				stream.Write(command, 0, command.Length);
				LogEvent("TX: " + Helpers.BytesToString(command), LogType.REGULAR);

				Task.Delay(Configuration.PacketTimeout).Wait();

				byte[] buffer = new byte[size];
				stream.Read(buffer, 0, buffer.Length);
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

			byte[] Command54(byte[] command, byte[] data)
			{
				ushort[] polynom_0x1021 = new ushort[]
				{
					0x0000, 0x1021, 0x2042, 0x3063, 0x4084, 0x50A5, 0x60C6, 0x70E7,
					0x8108, 0x9129, 0xA14A, 0xB16B, 0xC18C, 0xD1AD, 0xE1CE, 0xF1EF,
					0x1231, 0x0210, 0x3273, 0x2252, 0x52B5, 0x4294, 0x72F7, 0x62D6,
					0x9339, 0x8318, 0xB37B, 0xA35A, 0xD3BD, 0xC39C, 0xF3FF, 0xE3DE,
					0x2462, 0x3443, 0x0420, 0x1401, 0x64E6, 0x74C7, 0x44A4, 0x5485,
					0xA56A, 0xB54B, 0x8528, 0x9509, 0xE5EE, 0xF5CF, 0xC5AC, 0xD58D,
					0x3653, 0x2672, 0x1611, 0x0630, 0x76D7, 0x66F6, 0x5695, 0x46B4,
					0xB75B, 0xA77A, 0x9719, 0x8738, 0xF7DF, 0xE7FE, 0xD79D, 0xC7BC,
					0x48C4, 0x58E5, 0x6886, 0x78A7, 0x0840, 0x1861, 0x2802, 0x3823,
					0xC9CC, 0xD9ED, 0xE98E, 0xF9AF, 0x8948, 0x9969, 0xA90A, 0xB92B,
					0x5AF5, 0x4AD4, 0x7AB7, 0x6A96, 0x1A71, 0x0A50, 0x3A33, 0x2A12,
					0xDBFD, 0xCBDC, 0xFBBF, 0xEB9E, 0x9B79, 0x8B58, 0xBB3B, 0xAB1A,
					0x6CA6, 0x7C87, 0x4CE4, 0x5CC5, 0x2C22, 0x3C03, 0x0C60, 0x1C41,
					0xEDAE, 0xFD8F, 0xCDEC, 0xDDCD, 0xAD2A, 0xBD0B, 0x8D68, 0x9D49,
					0x7E97, 0x6EB6, 0x5ED5, 0x4EF4, 0x3E13, 0x2E32, 0x1E51, 0x0E70,
					0xFF9F, 0xEFBE, 0xDFDD, 0xCFFC, 0xBF1B, 0xAF3A, 0x9F59, 0x8F78,
					0x9188, 0x81A9, 0xB1CA, 0xA1EB, 0xD10C, 0xC12D, 0xF14E, 0xE16F,
					0x1080, 0x00A1, 0x30C2, 0x20E3, 0x5004, 0x4025, 0x7046, 0x6067,
					0x83B9, 0x9398, 0xA3FB, 0xB3DA, 0xC33D, 0xD31C, 0xE37F, 0xF35E,
					0x02B1, 0x1290, 0x22F3, 0x32D2, 0x4235, 0x5214, 0x6277, 0x7256,
					0xB5EA, 0xA5CB, 0x95A8, 0x8589, 0xF56E, 0xE54F, 0xD52C, 0xC50D,
					0x34E2, 0x24C3, 0x14A0, 0x0481, 0x7466, 0x6447, 0x5424, 0x4405,
					0xA7DB, 0xB7FA, 0x8799, 0x97B8, 0xE75F, 0xF77E, 0xC71D, 0xD73C,
					0x26D3, 0x36F2, 0x0691, 0x16B0, 0x6657, 0x7676, 0x4615, 0x5634,
					0xD94C, 0xC96D, 0xF90E, 0xE92F, 0x99C8, 0x89E9, 0xB98A, 0xA9AB,
					0x5844, 0x4865, 0x7806, 0x6827, 0x18C0, 0x08E1, 0x3882, 0x28A3,
					0xCB7D, 0xDB5C, 0xEB3F, 0xFB1E, 0x8BF9, 0x9BD8, 0xABBB, 0xBB9A,
					0x4A75, 0x5A54, 0x6A37, 0x7A16, 0x0AF1, 0x1AD0, 0x2AB3, 0x3A92,
					0xFD2E, 0xED0F, 0xDD6C, 0xCD4D, 0xBDAA, 0xAD8B, 0x9DE8, 0x8DC9,
					0x7C26, 0x6C07, 0x5C64, 0x4C45, 0x3CA2, 0x2C83, 0x1CE0, 0x0CC1,
					0xEF1F, 0xFF3E, 0xCF5D, 0xDF7C, 0xAF9B, 0xBFBA, 0x8FD9, 0x9FF8,
					0x6E17, 0x7E36, 0x4E55, 0x5E74, 0x2E93, 0x3EB2, 0x0ED1, 0x1EF0
				};

				byte[] body = new byte[command.Length + data.Length + 8];
				body[0] = DeviceNumber[0];
				body[1] = DeviceNumber[1];
				body[2] = StationNumber[0];
				body[3] = StationNumber[1];
				for (int i = 0; i < command.Length; i++) body[4 + i] = command[i];
				body[4 + command.Length] = Password[0];
				body[5 + command.Length] = Password[1];
				body[6 + command.Length] = Password[2];
				body[7 + command.Length] = Password[3];
				for (int i = 0; i < data.Length; i++) body[8 + command.Length + i] = data[i];

				byte[] tx = new byte[2 + body.Length + 3];
				tx[0] = 0xC0;
				tx[1] = 0x54;
				for (int i = 0; i < body.Length; i++)
				{
					tx[2 + i] = body[i];
				}
				tx[tx.Length - 1] = 0xC0;

				ushort crc = 0xFB81;  // Экспериментально найденное значение
				for (byte i = 0; i < body.Length; i++)
				{
					crc = (ushort)((crc << 8) ^ polynom_0x1021[(crc >> 8) ^ body[i]]);
				}

				byte[] answer = BitConverter.GetBytes(crc);
				tx[tx.Length - 2] = answer[0];
				tx[tx.Length - 3] = answer[1];

				return tx;
			}

			int Hex(byte b)
			{
				return int.TryParse(b.ToString("X2"), out int i) ? i : 0;
			}
		}
	}
}