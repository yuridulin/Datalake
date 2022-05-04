using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Timers;

namespace iNOPC.Drivers.TestDriver
{
    public class Driver : IDriver
    {
        public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

        public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

        public event LogEvent LogEvent;

        public event UpdateEvent UpdateEvent;

        public bool Start(string jsonConfig)
        {
            LogEvent("Запуск...");

            Fields = new Dictionary<string, DefField>();

            try
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
            }
            catch (Exception e)
            {
                LogEvent("Конфигурация не прочитана: " + e.Message, LogType.ERROR);
                return false;
            }

            foreach (var field in Configuration.Fields)
            {
                Fields.Add(field.Name, new DefField { Value = null, Quality = 0 });
            }

            Timer = new Timer(Configuration.Tick);
            Timer.Elapsed += (s, e) => Update();
            Timer.Start();

            Fields["Connection"] = new DefField { Value = true, Quality = 192 };
            Fields["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 };
            UpdateEvent();

            LogEvent("Мониторинг активен");

            return true;
        }

        public void Stop()
        {
            LogEvent("Остановка...");

            Fields["Connection"] = new DefField { Value = false, Quality = 192 };

            try
            {
                Timer?.Stop();
                Timer = null;
            } catch { }

            LogEvent("Мониторинг остановлен");
        }

        public void Write(string fieldName, object value)
        {
            Fields[fieldName].Value = value;
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
        }

        Configuration Configuration { get; set; } = new Configuration();

        Timer Timer { get; set; }

        void Update()
        {
            var r = new Random();

            foreach (var field in Configuration.Fields)
            {
                Fields[field.Name].Value = r.Next(0, 9999999);
                Fields[field.Name].Quality = 192;
            }

            Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
            Fields["Time"].Value = 192;

            LogEvent("Очередной опрос");
            UpdateEvent();
        }
    }
}