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

		public int NextId { get; set; } = 0;

		public void RestoreFromFile()
		{
			string raw;

			try
			{
				raw = File.ReadAllText(Path);
			}
			catch (Exception e)
			{
				Program.Log("Файл конфигурации \"" + Path + "\" не прочитан: " + e.Message);
				return;
			}

			try
			{
				var drivers = JsonConvert.DeserializeObject<List<Driver>>(raw);

				Drivers = drivers;
				Access = new List<AccessRecord>();

				Preprocess();
				return;
			}
			catch
			{ /* Если произошла ошибка, то конфиг более новый либо битый */ }

			try
			{
				var conf = JsonConvert.DeserializeObject<V1>(raw);

				Drivers = conf.Drivers;
				Access = conf.Access;
				
				Preprocess();
				return;
			}
			catch (Exception e)
			{
				Program.Log("Не удалось преобразовать конфигурацию к виду V1 \"" + Path + "\": " + e.Message);
				return;
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

						driver.Load();
					}

					Program.Log("Конфигурация загружена");
				}
				catch (Exception e)
				{
					Program.Log("Не удалось прочесть конфигурацию из файла \"" + Path + "\": " + e.Message);
				}
			}
		}

		public void SaveToFile()
		{
			// Конфиг сохраняется как V1
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
					Access,
					Version = "1",
				}));
			}
			catch (Exception e)
			{
				Program.Log("Не удалось записать конфигурацию в файл \"" + Path + "\": " + e.Message);
			}
		}
	}
}