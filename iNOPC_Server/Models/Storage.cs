using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace iNOPC.Server.Models
{
    public static class Storage
    {
        public static List<Driver> Drivers { get; set; } = new List<Driver>();

        public static int NextId { get; set; } = 0;

        public static string Path { get; set; } = Program.Base + @"\Configs\config.json";

        public static void Load()
        {
            try
            {
                Drivers = JsonConvert.DeserializeObject<List<Driver>>(File.ReadAllText(Path));
                NextId = 0;

                foreach (var driver in Drivers)
                {
                    driver.Id = ++NextId;

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
                Program.Err("Error: Не удалось получить конфиг по пути \"" + Path + "\"" +
                    "\nMessage: " + e.Message +
                    "\nStackTrace: " + e.StackTrace +
                    "\nSource: " + e.Source);
            }
        }

        public static void Save()
        {
            try
            {
                File.WriteAllText(Path, JsonConvert.SerializeObject(Drivers.Select(driver => new
                {
                    driver.Name,
                    driver.Path,
                    Devices = driver.Devices.Select(device => new
                    {
                        device.Name,
                        device.Active,
                        device.Configuration,
                    })
                })));
            }
            catch (Exception e)
            {
                Program.Err("Error: Не удалось сохранить конфиг по пути \"" + Path + "\"" +
                    "\nMessage: " + e.Message +
                    "\nStackTrace: " + e.StackTrace +
                    "\nSource: " + e.Source);
            }
        }
    }
}