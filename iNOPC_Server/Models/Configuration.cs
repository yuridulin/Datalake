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
		static string Path { get; set; } = Program.Base + @"\Configs\config.json";

		public List<Driver> Drivers { get; set; } = new List<Driver>();

		public List<AccessRecord> Access { get; set; } = new List<AccessRecord>();

		public Settings Settings { get; set; } = new Settings();

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
						Settings = new Settings();
						Preprocess();
						SaveToFile();
						break;

					case "2":
						var v2 = JsonConvert.DeserializeObject<V2>(raw);
						Drivers = v2.Drivers;
						Access = v2.Access;
						Settings = v2.Settings;
						Preprocess();
						break;

					default:
						Program.Log("Неизвестный формат конфигурации! " + version.Version);
						break;
				}
			}
			catch
			{
				// обработка V0 (конфиг является массивом драйверов)
				try
				{
					var drivers = JsonConvert.DeserializeObject<List<Driver>>(raw);
					Drivers = drivers;
					Access = new List<AccessRecord>();
					Settings = new Settings();
					Preprocess();
					SaveToFile();
				}
				catch (Exception e)
				{
					Program.Log("Неизвестный формат конфигурации: " + e.Message);
				}
			}

			void Preprocess()
			{
				try
				{
					NextId = 0;

					foreach (var driver in Drivers)
					{
						driver.Id = +NextId;

						foreach (var device in driver.Devices)
						{
							device.Id = ++NextId;
							device.DriverId = driver.Id;
							device.DriverName = driver.Name;
						}

						//driver.Load();
					}

					Program.Log("Конфигурация загружена");
				}
				catch (Exception e)
				{
					Program.Log("Не удалось прочесть конфигурацию из файла \"" + Path + "\": " + e.Message);
				}

				try
				{
					File.WriteAllText(Program.Base + "\\webConsole\\js\\settings.js", 
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
			// Конфиг сохраняется как V2
			try
			{
				File.WriteAllText(Path, JsonConvert.SerializeObject(new
				{
					Drivers = Drivers
						.Select(driver => new
						{
							driver.Name,
							driver.Path,
							Devices = driver.Devices
								.Select(device => new
								{
									device.Name,
									device.Active,
									device.AutoStart,
									device.Configuration,
								})
								.ToList()
						})
						.ToList(),
					Settings,
					Access,
					Version = "2",
				}));
			}
			catch (Exception e)
			{
				Program.Log("Не удалось записать конфигурацию в файл \"" + Path + "\": " + e.Message);
			}
		}
	}
}