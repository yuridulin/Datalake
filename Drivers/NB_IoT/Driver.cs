using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
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

					//var buffer = new List<byte>();
					byte[] bytes = new byte[7];

					bool isReadingDone = Task
						.Run(() => 
						{
							// Чтение произвольного количества байт
							//while (stream.DataAvailable)
							//{
							//	int b = stream.ReadByte();
							//	if (b < 0) break;
							//	buffer.Add((byte)b);
							//}

							stream.Read(bytes, 0, 7);
						})
						.Wait(TimeSpan.FromSeconds(3));

					if (!isReadingDone)
					{
						LogEvent("Чтение потока прервано по таймауту");
					}

					//ParseValue(buffer.ToArray());
					ParseValue(bytes);

					//LogEvent("Получено: " + Helpers.BytesToString(buffer.ToArray()));
					LogEvent("Получено: " + Helpers.BytesToString(bytes));

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
			switch (packet[0])
			{
				case 1:
					byte address = packet[ 1 ];
					uint dateUint = BitConverter.ToUInt32(new[] { packet[ 3 ], packet[ 4 ], packet[ 5 ], packet[ 6 ] }, 0);
					DateTime date = ToDateTime(dateUint);

					SetValue(address + ".Date", date.ToString("dd.MM.yyyy HH:mm:ss"));
					SetValue(address + ".Value", packet[ 6 ]);

					LastUpdateTime = DateTime.Now;
					break;

				case 2:
					BitArray bits = new BitArray(new byte[] { packet[ 2 ] });
					SetValue(packet[ 1 ] + ".Bit1", bits[ 5 ]);
					SetValue(packet[ 1 ] + ".Bit2", bits[ 4 ]);
					SetValue(packet[ 1 ] + ".Bit3", bits[ 3 ]);
					SetValue(packet[ 1 ] + ".Bit4", bits[ 2 ]);
					SetValue(packet[ 1 ] + ".Bit5", bits[ 1 ]);
					SetValue(packet[ 1 ] + ".Bit6", bits[ 0 ]);

					LastUpdateTime = DateTime.Now;
					break;

				default:
					LogEvent("Получен пакет, тип которого не распознан", LogType.WARNING);
					break;
			}
		}

		DateTime ToDateTime(uint value)
		{
			int RTC_MODE2_CLOCK_SECOND_Pos = 0;
			int RTC_MODE2_CLOCK_SECOND_Msk = 0x3F << RTC_MODE2_CLOCK_SECOND_Pos;

			int RTC_MODE2_CLOCK_MINUTE_Pos = 6;
			int RTC_MODE2_CLOCK_MINUTE_Msk = 0x3F << RTC_MODE2_CLOCK_MINUTE_Pos;

			int RTC_MODE2_CLOCK_HOUR_Pos = 12;
			int RTC_MODE2_CLOCK_HOUR_Msk = 0x1F << RTC_MODE2_CLOCK_HOUR_Pos;

			int RTC_MODE2_CLOCK_DAY_Pos = 17;
			int RTC_MODE2_CLOCK_DAY_Msk = 0x1F << RTC_MODE2_CLOCK_DAY_Pos;

			int RTC_MODE2_CLOCK_MONTH_Pos = 22;
			int RTC_MODE2_CLOCK_MONTH_Msk = 0xF << RTC_MODE2_CLOCK_MONTH_Pos;

			int RTC_MODE2_CLOCK_YEAR_Pos = 26;
			int RTC_MODE2_CLOCK_YEAR_Msk = 0x3F << RTC_MODE2_CLOCK_YEAR_Pos;

			int year = (int)((value & RTC_MODE2_CLOCK_YEAR_Msk) >> RTC_MODE2_CLOCK_YEAR_Pos) + 2000;
			int month = (int)((value & RTC_MODE2_CLOCK_MONTH_Msk) >> RTC_MODE2_CLOCK_MONTH_Pos);
			int day = (int)((value & RTC_MODE2_CLOCK_DAY_Msk) >> RTC_MODE2_CLOCK_DAY_Pos);
			int hour = (int)((value & RTC_MODE2_CLOCK_HOUR_Msk) >> RTC_MODE2_CLOCK_HOUR_Pos);
			int minute = (int)((value & RTC_MODE2_CLOCK_MINUTE_Msk) >> RTC_MODE2_CLOCK_MINUTE_Pos);
			int second = (int)((value & RTC_MODE2_CLOCK_SECOND_Msk) >> RTC_MODE2_CLOCK_SECOND_Pos);
			return new DateTime(year, month, day, hour, minute, second);
		}

		DateTime LastUpdateTime { get; set; }
	}
}
