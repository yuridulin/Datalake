using iNOPC.Server.Models.Configurations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iNOPC.Server.Models
{
	public class Configuration
	{
		static string Path { get; set; } = Program.Base + @"Configs\config.json";

		public List<Driver> Drivers { get; set; } = new List<Driver>();

		public List<AccessRecord> Access { get; set; } = new List<AccessRecord>();

		public Settings Settings { get; set; } = new Settings();

		public DatabaseSettings Database { get; set; } = new DatabaseSettings();

		public List<MathField> MathFields { get; set; } = new List<MathField>();

		public string Key { get; set; } = "";

		public int NextId { get; set; } = 0;

		public void RestoreFromFile()
		{
			string raw;

			// чтение из файла
			try
			{
				raw = File.ReadAllText(Path);
			}
			catch (Exception e)
			{
				Program.Log("Файл конфигурации \"" + Path + "\" не прочитан: " + e.Message);
				return;
			}

			// попытка прочесть версию конфига
			try
			{
				var version = JsonConvert.DeserializeObject<V0>(raw);

				switch (version.Version)
				{
					case "1":
						var v1 = JsonConvert.DeserializeObject<V1>(raw);
						Drivers = v1.Drivers;
						Access = v1.Access;
						break;

					case "2":
						var v2 = JsonConvert.DeserializeObject<V2>(raw);
						Drivers = v2.Drivers;
						Access = v2.Access;
						Settings = v2.Settings;
						Key = v2.Key;
						break;

					case "3":
						var v3 = JsonConvert.DeserializeObject<V3>(raw);
						Drivers = v3.Drivers;
						Access = v3.Access;
						Settings = v3.Settings;
						Database = v3.Database;
						Key = v3.Key;
						break;

					case "4":
						var v4 = JsonConvert.DeserializeObject<V4>(raw);
						Drivers = v4.Drivers;
						Access = v4.Access;
						Settings = v4.Settings;
						Database = v4.Database;
						MathFields = v4.MathFields;
						Key = v4.Key;
						break;

					default:
						Program.Log("Неизвестный формат конфигурации! " + version.Version);
						return;
				}
			}
			catch
			{
				// обработка V0 (конфиг является массивом драйверов)
				try
				{
					var drivers = JsonConvert.DeserializeObject<List<Driver>>(raw);
					Drivers = drivers;
				}
				catch (Exception e)
				{
					Program.Log("Неизвестный формат конфигурации: " + e.Message);
				}
			}

			Preprocess();
			SaveToFile();

			void Preprocess()
			{
				try
				{
					// прогон для поиска идентификаторов
					var ids = new List<int> { 1 };

					foreach (var driver in Drivers)
					{
						if (driver.Id != 0) ids.Add(driver.Id);
						foreach (var device in driver.Devices)
						{
							if (device.Id != 0) ids.Add(device.Id);
						}
					}

					NextId = (ids?.Max() ?? 0) + 1;

					foreach (var driver in Drivers)
					{
						driver.Id = driver.Id == 0 ? NextId++ : driver.Id;
						foreach (var device in driver.Devices)
						{
							device.Id = device.Id == 0 ? NextId++ : device.Id;
							device.DriverId = driver.Id;
							device.DriverName = driver.Name;
						}
					}

					Program.Log("Конфигурация загружена");
				}
				catch (Exception e)
				{
					Program.Log("Не удалось прочесть конфигурацию из файла \"" + Path + "\": " + e.Message);
				}

				try
				{
					File.WriteAllText(Program.Base + "webConsole\\js\\settings.js", 
						"var _httpPort = " + Settings.WebConsolePort + "\nvar _wsPort = " + Settings.WebConsoleSocketPort);
				}
				catch (Exception e)
				{
					Program.Log("Не удалось записать конфигурацию в веб-консоль по адресу \"" + Program.Base + "\\webConsole\\js\\settings.js\": " + e.Message);
				}
			}
		}

		public void Start()
		{
			foreach (var driver in Drivers)
			{
				driver.Load();
			}
		}

		public void SaveToFile()
		{
			// Конфиг сохраняется как последняя версия
			try
			{
				File.WriteAllText(Path, JsonConvert.SerializeObject(new
				{
					Drivers = Drivers
						.Select(driver => new
						{
							driver.Id,
							driver.Name,
							driver.Path,
							Devices = driver.Devices
								.Select(device => new
								{
									device.Id,
									device.Name,
									device.Active,
									device.AutoStart,
									device.Configuration,
								})
								.ToList()
						})
						.ToList(),
					Access,
					Settings,
					Database,
					MathFields = MathFields
						.Select(mathfield => new
						{
							mathfield.Name,
							mathfield.Type,
							mathfield.Fields,
							mathfield.DefValue,
						})
						.ToList(),
					Key,
					Version = "4",
				}));
			}
			catch (Exception e)
			{
				Program.Log("Не удалось записать конфигурацию в файл \"" + Path + "\": " + e.Message);
			}
		}
	}
}