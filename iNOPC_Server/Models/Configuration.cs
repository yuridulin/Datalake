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
			try
			{
				var conf = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(Path));
				Drivers = conf.Drivers;
				Access = conf.Access;
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
			}
			catch (Exception e)
			{
				Console.WriteLine("Не удалось прочесть конфигурацию из файла \"" + Path + "\": " + e.Message);
			}
		}

		public void SaveToFile()
		{
			try
			{
				File.WriteAllText(Path, JsonConvert.SerializeObject(new
				{
					Drivers = Drivers.Select(driver => new
					{
						driver.Name,
						driver.Path,
						Devices = driver.Devices.Select(device => new
						{
							device.Name,
							device.Active,
							device.AutoStart,
							device.Configuration,
						})
					}),
					Access,
				}));
			}
			catch (Exception e)
			{
				Console.WriteLine("Не удалось записать конфигурацию в файл \"" + Path + "\": " + e.Message);
			}
		}
	}
}