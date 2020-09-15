using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Timers;

namespace iNOPC.Drivers.TestDriver
{
    public class Driver : IDriver
    {
        public Dictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();

        public event LogEvent LogEvent;

        public event UpdateEvent UpdateEvent;

        public event WinLogEvent WinLogEvent;

        public bool Start(string jsonConfig)
        {
            LogEvent("Запуск...");

            Fields = new Dictionary<string, object>();

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
                Fields.Add(field.Name, null);
            }

            Timer = new Timer(Configuration.Tick);
            Timer.Elapsed += (s, e) => Update();
            Timer.Start();

            Fields["Connection"] = true;

            LogEvent("Мониторинг активен");

            return true;
        }

        public void Stop()
        {
            LogEvent("Остановка...");

            Fields["Connection"] = false;

            try
            {
                Timer?.Stop();
                Timer = null;
            } catch { }

            LogEvent("Мониторинг остановлен");
        }

        public void Write(string fieldName, object value)
        {
            Fields[fieldName] = value;
            WinLogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
        }

        Configuration Configuration { get; set; } = new Configuration();

        Timer Timer { get; set; }

        void Update()
        {
            var r = new Random();

            foreach (var field in Configuration.Fields)
            {
                Fields[field.Name] = r.Next();
            }

            Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");

            UpdateEvent();
        }
    }
}