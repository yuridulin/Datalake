using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace iNOPC.Drivers.MT01
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

			try { Port?.Close(); } catch (Exception) { }
			Port = new SerialPort();
			Port.DataReceived += (s, e) =>
			{
				BytesReceived = Port.BytesToRead;
				Receive = true;
			};
			Port.ErrorReceived += (s, e) => ComError(e.EventType.ToString());

			// чтение конфигурации
			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
			}
			catch (Exception e)
			{
				return Err("Конфигурация не прочитана: " + e.Message);
			}

			if (!Fields.ContainsKey("Time"))
			{
				Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 });
			}

			UpdateEvent();

			// установка начальных значений
			try
			{
				Port.PortName = "COM" + Configuration.Port;
				Port.BaudRate = Configuration.BaudRate;
				Port.DataBits = Configuration.DataBits;
				Port.Parity = (Parity)Configuration.Parity;
				Port.StopBits = (StopBits)Configuration.StopBits;
			}
			catch (Exception e)
			{
				return Err("Параметры COM порта не установлены: " + e.Message + "\n" + e.StackTrace);
			}

			Active = true;

			if (Thread != null)
			{
				Thread.Abort();
				Thread = null;
			}

			Thread = new Thread(TrancievePackages);
			Thread.Start();

			LogEvent("Мониторинг запущен");

			return true;
		}

		public void Stop()
		{
			LogEvent("Остановка ...");

			Active = false;
			try { Port.Close(); } catch (Exception) { }

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
		}


		// Реализация получения данных

		Configuration Configuration { get; set; }

		SerialPort Port { get; set; }

		Thread Thread { get; set; }

		DateTime Date { get; set; }

		DateTime PackageDate { get; set; }

		bool Active { get; set; } = false;

		bool Receive { get; set; } = false;

		int BytesReceived { get; set; } = 0;

		int ErrorsCount { get; set; } = 0;

		void TrancievePackages()
		{
			while (Active)
			{
				Date = DateTime.Now;

				if (!Port.IsOpen)
				{
					try
					{
						Port.Open();
					}
					catch (Exception e)
					{
						Err("COM-порт не открыт: " + e.Message + "\n" + e.StackTrace);
						try { Port?.Close(); } catch (Exception) { }
						Thread.Sleep(1000);
					}
				}

				var receivedValues = new Dictionary<string, object>();

				if (Port.IsOpen)
				{
					ErrorsCount = 0;

					if (Configuration.CheckCurrent)
					{
						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x01, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 4);
							receivedValues.Add("Energy.Now", BitConverter.ToUInt32(data, 0));
						}

						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x14, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 4);
							receivedValues.Add("DeviceTime", UnixTimeStampToDateTime(BitConverter.ToUInt32(data, 0)).ToString("dd.MM.yyyy HH:mm:ss"));
						}
					}

					if (Configuration.CheckParams)
					{
						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x02, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 24);
							receivedValues.Add("Line.Power.A.Active", BitConverter.ToUInt32(data, 0));
							receivedValues.Add("Line.Power.A.Reactive", BitConverter.ToUInt32(data, 4));
							receivedValues.Add("Line.Power.B.Active", BitConverter.ToUInt32(data, 8));
							receivedValues.Add("Line.Power.B.Reactive", BitConverter.ToUInt32(data, 12));
							receivedValues.Add("Line.Power.C.Active", BitConverter.ToUInt32(data, 16));
							receivedValues.Add("Line.Power.C.Reactive", BitConverter.ToUInt32(data, 20));
						}

						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x03, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 12);
							receivedValues.Add("Line.U.A", BitConverter.ToUInt32(data, 0));
							receivedValues.Add("Line.U.B", BitConverter.ToUInt32(data, 4));
							receivedValues.Add("Line.U.C", BitConverter.ToUInt32(data, 8));
						}

						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x04, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 12);
							receivedValues.Add("Line.I.A", BitConverter.ToUInt32(data, 0));
							receivedValues.Add("Line.I.B", BitConverter.ToUInt32(data, 4));
							receivedValues.Add("Line.I.C", BitConverter.ToUInt32(data, 8));
						}

						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x05, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 12);
							receivedValues.Add("Line.PowerCoef.A", BitConverter.ToUInt32(data, 0));
							receivedValues.Add("Line.PowerCoef.B", BitConverter.ToUInt32(data, 4));
							receivedValues.Add("Line.PowerCoef.C", BitConverter.ToUInt32(data, 8));
						}

						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x06, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 12);
							receivedValues.Add("Line.Frequency.A", BitConverter.ToUInt32(data, 0));
							receivedValues.Add("Line.Frequency.B", BitConverter.ToUInt32(data, 4));
							receivedValues.Add("Line.Frequency.C", BitConverter.ToUInt32(data, 8));
						}
					}

					if (Configuration.CheckInfo)
					{
						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x07, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 4);
							receivedValues.Add("Info.DeviceType", Encoding.UTF8.GetString(data));
						}

						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x08, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 4);
							receivedValues.Add("Info.SerialNumber", BitConverter.ToUInt32(data, 0));
						}

						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x09, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 4);
							receivedValues.Add("Info.ManufactureDate", new DateTime(2000 + data[3], data[2], data[1]).ToString("dd.MM.yyyy"));
						}

						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x0A, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 4);
							receivedValues.Add("Info.SoftVersion", data[0] + "." + data[1] + "." + data[2] + "." + data[3]);
						}

						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x0B, new byte[] { 0x0, 0x0, 0x0, 0x0 }, 4);
							receivedValues.Add("Info.Address", data[0]);
						}
					}

					if (Configuration.CheckDay)
					{
						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x11, new byte[] { 0x2, 0x0, 0x0, 0x0 }, 10);
							receivedValues.Add("Energy.Day", BitConverter.ToUInt32(data, 6));
						}
					}

					if (Configuration.CheckMonth)
					{
						if (Active && (ErrorsCount < 3))
						{
							byte[] data = Exchange(0x12, new byte[] { 0x2, 0x0, 0x0, 0x0 }, 10);
							receivedValues.Add("Energy.Day", BitConverter.ToUInt32(data, 6));
						}
					}
				}

				if (ErrorsCount >= 3)
				{
					try { Port?.Close(); } catch (Exception) { }
					ErrorsCount = 0;
					Thread.Sleep(1000);
				}
				else
				{
					lock (Fields)
					{
						Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
						Fields["Time"].Quality = 192;

						foreach (var value in receivedValues)
						{
							Fields[value.Key].Value = value.Value;
							Fields[value.Key].Quality = 192;
						}
					}

					UpdateEvent();
				}

				int timeout = Convert.ToInt32((Configuration.CyclicIntervalInSeconds * 1000) - (DateTime.Now - Date).TotalMilliseconds);
				if (timeout > 0) Thread.Sleep(timeout);

				try { Port?.Close(); } catch (Exception) { }
			}
		}

		void ComError(string errorName)
		{
			LogEvent("COM: " + errorName, LogType.DETAILED);
			Port.BaseStream.Flush();
			Port.DiscardInBuffer();
			Port.DiscardOutBuffer();
		}

		bool Err(string text)
		{
			LogEvent(text, LogType.ERROR);
			return false;
		}

		byte CRC(byte[] command)
		{
			byte[] crc8Table =
			{
				0x31, 0x07, 0x0E, 0x09, 0x1C, 0x1B, 0x12, 0x15, 0x38, 0x3F, 0x36, 0x31, 0x24, 0x23, 0x2A, 0x2D,
				0x70, 0x77, 0x7E, 0x79, 0x6C, 0x6B, 0x62, 0x65, 0x48, 0x4F, 0x46, 0x41, 0x54, 0x53, 0x5A, 0x5D,
				0xE0, 0xE7, 0xEE, 0xE9, 0xFC, 0xFB, 0xF2, 0xF5, 0xD8, 0xDF, 0xD6, 0xD1, 0xC4, 0xC3, 0xCA, 0xCD,
				0x90, 0x97, 0x9E, 0x99, 0x8C, 0x8B, 0x82, 0x85, 0xA8, 0xAF, 0xA6, 0xA1, 0xB4, 0xB3, 0xBA, 0xBD,
				0xC7, 0xC0, 0xC9, 0xCE, 0xDB, 0xDC, 0xD5, 0xD2, 0xFF, 0xF8, 0xF1, 0xF6, 0xE3, 0xE4, 0xED, 0xEA,
				0xB7, 0xB0, 0xB9, 0xBE, 0xAB, 0xAC, 0xA5, 0xA2, 0x8F, 0x88, 0x81, 0x86, 0x93, 0x94, 0x9D, 0x9A,
				0x27, 0x20, 0x29, 0x2E, 0x3B, 0x3C, 0x35, 0x32, 0x1F, 0x18, 0x11, 0x16, 0x03, 0x04, 0x0D, 0x0A,
				0x57, 0x50, 0x59, 0x5E, 0x4B, 0x4C, 0x45, 0x42, 0x6F, 0x68, 0x61, 0x66, 0x73, 0x74, 0x7D, 0x7A,
				0x89, 0x8E, 0x87, 0x80, 0x95, 0x92, 0x9B, 0x9C, 0xB1, 0xB6, 0xBF, 0xB8, 0xAD, 0xAA, 0xA3, 0xA4,
				0xF9, 0xFE, 0xF7, 0xF0, 0xE5, 0xE2, 0xEB, 0xEC, 0xC1, 0xC6, 0xCF, 0xC8, 0xDD, 0xDA, 0xD3, 0xD4,
				0x69, 0x6E, 0x67, 0x60, 0x75, 0x72, 0x7B, 0x7C, 0x51, 0x56, 0x5F, 0x58, 0x4D, 0x4A, 0x43, 0x44,
				0x19, 0x1E, 0x17, 0x10, 0x05, 0x02, 0x0B, 0x0C, 0x21, 0x26, 0x2F, 0x28, 0x3D, 0x3A, 0x33, 0x34,
				0x4E, 0x49, 0x40, 0x47, 0x52, 0x55, 0x5C, 0x5B, 0x76, 0x71, 0x78, 0x7F, 0x6A, 0x6D, 0x64, 0x63,
				0x3E, 0x39, 0x30, 0x37, 0x22, 0x25, 0x2C, 0x2B, 0x06, 0x01, 0x08, 0x0F, 0x1A, 0x1D, 0x14, 0x13,
				0xAE, 0xA9, 0xA0, 0xA7, 0xB2, 0xB5, 0xBC, 0xBB, 0x96, 0x91, 0x98, 0x9F, 0x8A, 0x8D, 0x84, 0x83,
				0xDE, 0xD9, 0xD0, 0xD7, 0xC2, 0xC5, 0xCC, 0xCB, 0xE6, 0xE1, 0xE8, 0xEF, 0xFA, 0xFD, 0xF4, 0xF3
			};

			byte crc = 0x00;

			for (int i = 0; i < command.Length; i++)
				crc = crc8Table[crc ^ command[i]];

			return crc;
		}

		byte[] Exchange(byte command, byte[] data, byte len)
		{
			byte[] answer = new byte[0];

			if (!Active) return new byte[0];
			if (ErrorsCount > 3)
			{
				try { Port?.Close(); } catch (Exception) { }
				return new byte[0];
			}
			try
			{
				Receive = false;
				BytesReceived = 0;
				PackageDate = DateTime.Now;

				byte crc = CRC(new byte[] { Configuration.Address, 0x1, command, data[0], data[1], data[2], data[3] });
				byte[] tx = new byte[] { Configuration.Address, 0x1, command, data[0], data[1], data[2], data[3], crc };
				Port.Write(tx, 0, tx.Length);
				LogEvent("Tx: " + Helpers.BytesToString(tx), LogType.DETAILED);

				while ((!Receive || BytesReceived < len) && (DateTime.Now - PackageDate).TotalMilliseconds < Configuration.ReceiveTimeoutInMilliseconds)
				{
					Thread.Sleep(1);
				}

				if (Receive)
				{
					byte[] rx = new byte[Port.BytesToRead];
					Port.Read(rx, 0, rx.Length);
					LogEvent("Rx: " + Helpers.BytesToString(rx), LogType.DETAILED);

					// Проверка длины посылки
					if (rx.Length != len)
					{
						LogEvent("длина ответа не совпадает: пришло " + rx.Length + ", ожидается " + len + " байт", LogType.DETAILED);
					}
					// Проверка контрольной суммы
					else if (CRC(rx.Take(rx.Length - 1).ToArray()) != rx[rx.Length - 1])
					{
						LogEvent("CRC не совпадает с расчетным", LogType.DETAILED);
					}
					// Если все норм, выполняется разбор ответа
					else
					{
						answer = new byte[len - 4];
						for (int i = 0; i < answer.Length; i++)
						{
							answer[i] = rx[i + 3];
						}
					}
				}
				else
				{
					LogEvent("Rx: ничего не вернулось, таймаут " + Configuration.ReceiveTimeoutInMilliseconds + " мс", LogType.DETAILED);
					ErrorsCount++;
				}

				Thread.Sleep(5);
			}
			catch (Exception e)
			{
				LogEvent("Ошибка при опросе значений: " + e.Message + "\n" + e.StackTrace, LogType.DETAILED);
				ErrorsCount++;
			}

			return answer;
		}

		DateTime UnixTimeStampToDateTime(double unixTimeStamp)
		{
			// Unix timestamp is seconds past epoch
			DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
			return dateTime;
		}
	}
}
