using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Drivers.ASK_Binary
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

			// создание полей
			CreateValuesAndUpdate();

			// чтение конфигурации
			try
			{
				Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
			}
			catch (Exception e)
			{
				LogEvent("Конфигурация не прочитана: " + e.Message, LogType.ERROR);
				return false;
			}

			// запуск опроса
			Active = true;
			Timer = new Timer(Configuration.Timeout * 60000);
			Timer.Elapsed += (s, e) => { GetData(); };
			Timer.Start();

			// сигнализация о старте и первый опрос
			LogEvent("Мониторинг запущен");
			Task.Run(() => GetData());

			return true;
		}

		public void Stop()
		{
			LogEvent("Остановка ...");

			Active = false;
			try { Timer?.Stop(); } catch { }
			try { Timer = null; } catch { }
			ClearAllValuesAndUpdate();

			LogEvent("Мониторинг остановлен");
		}

		public void Write(string fieldName, object value)
		{
			LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
		}


		// Реализация получения данных

		Configuration Configuration { get; set; }

		Timer Timer { get; set; }

		bool Active { get; set; } = false;

		Dictionary<string, int> Params { get; set; } = new Dictionary<string, int>
		{
			{ "vibros.CO", 1 },
			{ "vibros.NOx", 2 },
			{ "vibros.SO2", 3 },
			{ "vibros.NO", 4 },

			{ "vybros3.Temp_A", 1 },
			{ "vybros3.Temp_B", 2 },
			{ "vybros3.Pressure_A", 3 },
			{ "vybros3.Pressure_B", 4 },
			{ "vybros3.Speed_A", 5 },
			{ "vybros3.Speed_B", 6 },
			{ "vybros3.MazutAmount_A", 7 },
			{ "vybros3.MazutAmount_B", 8 },
			{ "vybros3.O2_A", 9 },
			{ "vybros3.O2_B", 10 },
			{ "vybros3.CO_A", 11 },
			{ "vybros3.CO_B", 12 },
			{ "vybros3.NOx_A", 13 },
			{ "vybros3.NOx_B", 14 },
			{ "vybros3.SO2_A", 15 },
			{ "vybros3.SO2_B", 16 },
			{ "vybros3.CO2_A", 17 },
			{ "vybros3.CO2_B", 18 },
			{ "vybros3.NO_A", 19 },
			{ "vybros3.NO_B", 20 },

			{ "vybros1_analog.CO_A", 70 },
			{ "vybros1_analog.O2_A", 71 },
			{ "vybros1_analog.NO_A", 72 },
			{ "vybros1_analog.SO2_A", 74 },
			{ "vybros1_analog.CO2_A", 75 },
			{ "vybros1_analog.Pressure_A", 78 },
			{ "vybros1_analog.Temp_A", 79 },
			{ "vybros1_analog.CO_B", 80 },
			{ "vybros1_analog.O2_B", 81 },
			{ "vybros1_analog.NO_B", 82 },
			{ "vybros1_analog.SO2_B", 84 },
			{ "vybros1_analog.CO2_B", 85 },
			{ "vybros1_analog.Pressure_B", 88 },
			{ "vybros1_analog.Temp_B", 89 },
			{ "vybros1_analog.PressureBar", 109 },
			{ "vybros1_analog.GasAmount_A", 139 },
			{ "vybros1_analog.CO_A_2", 154 },
			{ "vybros1_analog.NOx_A", 165 },
			{ "vybros1_analog.NO_A_2", 170 },
			{ "vybros1_analog.SO2_A_2", 177 },
			{ "vybros1_analog.GasAmount_B", 191 },
			{ "vybros1_analog.CO_B_2", 212 },
			{ "vybros1_analog.NOx_B", 215 },
			{ "vybros1_analog.NO_B_2", 222 },
			{ "vybros1_analog.SO2_B_2", 228 },
		};

		void GetData()
		{
			if (!Active) return;

			DirectoryInfo dir;

			try
			{
				dir = new DirectoryInfo(Configuration.Path);

				LogEvent("Доступ к папке получен");
			}
			catch (Exception ex)
			{
				LogEvent("Доступ к папке: " + ex.Message, LogType.ERROR);
				ClearAllValuesAndUpdate();
				return;
			}

			try
			{
				float[] values;
				string[] files = new[] { "vybros1_analog.dat", "vybros3.dat", "vibros.dat" };

				foreach (var file in dir.GetFiles())
				{
					LogEvent("Работа с файлом " + file.Name);

					if (files.Contains(file.Name))
					{
						string fileName = file.Name.Replace(".dat", "");

						try
						{
							values = GetValuesFromFile(file.FullName);

							WriteValues(fileName, values);

							LogEvent("Данные прочитаны");
						}
						catch (Exception e)
						{
							LogEvent("Ошибка при чтении: " + e.Message, LogType.ERROR);

							ClearValues(fileName);
						}
					}
				}
			}
			catch (DirectoryNotFoundException ex)
			{
				LogEvent("Папка на найдена: " + ex.Message, LogType.ERROR);
			}

			Update();
		}

		float[] GetValuesFromFile(string path)
		{
			byte[] raw = File.ReadAllBytes(path);

			var arr = new List<float> { 0 };
			int i = 0;
			while ((i + 4) <= raw.Length)
			{
				arr.Add(BitConverter.ToSingle(raw, i));
				i += 4;
			}

			return arr.ToArray();
		}

		void WriteValues(string key, float[] values)
		{
			lock (Fields)
			{
				foreach (var kv in Params)
				{
					if (kv.Key.StartsWith(key))
					{
						Fields[kv.Key].Value = values[kv.Value];
						Fields[kv.Key].Quality = 192;
					}
				}
			}
		}

		void CreateValuesAndUpdate()
		{
			lock (Fields)
			{
				foreach (var kv in Params)
				{
					if (!Fields.ContainsKey(kv.Key))
					{
						Fields.Add(kv.Key, new DefField { Quality = 0, Value = 0f });
					}
					else
					{
						Fields[kv.Key].Value = 0f;
						Fields[kv.Key].Quality = 192;
					}
				}

				if (!Fields.ContainsKey("Time"))
				{
					Fields.Add("Time", new DefField { Quality = 192, Value = DateTime.Now.ToString("HH:mm:ss") });
				}
			}

			UpdateEvent();
		}

		void Update()
		{
			lock (Fields)
			{
				Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
				Fields["Time"].Quality = 192;
			}

			UpdateEvent();
		}

		void ClearValues(string key)
		{
			lock (Fields)
			{
				foreach (var kv in Fields)
				{
					if (kv.Key.StartsWith(key))
					{
						kv.Value.Quality = 0;
					}
				}
			}
		}

		void ClearAllValuesAndUpdate()
		{
			lock (Fields)
			{
				foreach (var kv in Fields)
				{
					kv.Value.Quality = 0;
				}
			}

			Update();
		}
	}
}
