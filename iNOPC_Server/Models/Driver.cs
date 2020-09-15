using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using iNOPC.Server.Web;
using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Server.Models
{
    public class Driver
    {
        // Параметры конфигурации

        public string Name { get; set; }

        public string Path { get; set; }

        public List<Device> Devices { get; set; } = new List<Device>();


        // Рабочие параметры

        public int Id { get; set; } = 0;

        public Type DriverType { get; set; }

        public MethodInfo ConfigurationPage { get; set; }

        public List<Log> Logs { get; set; } = new List<Log>();

        public string DefaultConfiguratuon { get; set; } = "{}";

        public Dictionary<string, string> DefaultFields { get; set; } = new Dictionary<string, string>();


        // Методы взаимодействия

        public bool Load()
        {
            try
            {
                // запоминаем активные в данный момент устройства, чтобы возобновить опросы
                var activeDevices = Devices.Where(x => x.Active).Select(x => x.Name).ToList();
                foreach (var device in Devices.Where(x => x.Active)) device.Stop();

                // загружаем код DLL сборки
                var bytes = File.ReadAllBytes(Path);
                var dll = Assembly.Load(bytes);

                // убираем предыдущую версию
                DriverType = null;
                DefaultFields.Clear();

                foreach (var type in dll.GetTypes())
                {
                    if (type.Name.Contains("Driver")) DriverType = type;
                    if (type.Name.Contains("Configuration"))
                    {
                        DefaultConfiguratuon = JsonConvert.SerializeObject(Activator.CreateInstance(type));
                        ConfigurationPage = type.GetMethod("GetPage", BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Static);
                    }
                }

                if (DriverType == null)
                {
                    throw new Exception("Тип драйвера не найден");
                }

                if (ConfigurationPage == null)
                {
                    throw new Exception("Метод для редактирования конфигурации не найден");
                }

                Log("Драйвер успешно загружен");

                // возобновляем опросы
                foreach (var device in Devices)
                {
                    if (activeDevices.Contains(device.Name))
                    {
                        device.Start();
                    }
                }

                // завершаем перезагрузку драйвера
                Update();
                return true;
            }
            catch (Exception e)
            {
                Log(e.Message, LogType.ERROR);
                return false;
            }
        }

        public void Update()
        {
            WebSocket.Broadcast("tree");
            WebSocket.Broadcast("driver.devices:" + Id);
            WebSocket.Broadcast("driver.logs:" + Id);
        }

        public void Log(string text, LogType type = LogType.REGULAR)
        {
            if (Logs.Count >= 100)
            {
                Logs.RemoveAt(0);
            }

            Logs.Add(new Log
            {
                Date = DateTime.Now,
                Text = text,
                Type = type
            });

            if (type == LogType.ERROR)
            {
                Program.Err("Error: " + text + "\nDriver: " + Name);
                WebSocket.Broadcast("tree");
            }

            WebSocket.Broadcast("driver.logs:" + Id);
        }
    }
}