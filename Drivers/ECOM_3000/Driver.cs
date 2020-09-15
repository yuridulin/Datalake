using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Drivers.ECOM_3000
{
    public class Driver : IDriver
    {
        public event LogEvent LogEvent;

        public event UpdateEvent UpdateEvent;

        public event WinLogEvent WinLogEvent;

        public Dictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();

        public bool Start(string jsonConfig)
        {
            LogEvent("Запуск ...");
            Fields.Clear();
            Fields.Add("Time", DateTime.Now.ToString("HH:mm:ss"));

            for (int k = 1; k < 270; k++)
            {
                Fields.Add("Hour." + k, 0);
                Fields.Add("Day." + k, 0);
            }

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

            Timer = new Timer(Configuration.Timeout);
            Timer.Elapsed += (s, e) => LoadData(false);
            Timer.Start();

            LogEvent("Мониторинг запущен");

            Task.Run(() => LoadData(true));

            return true;
        }

        public void Stop()
        {
            LogEvent("Остановка ...");

            Timer.Stop();

            LogEvent("Мониторинг остановлен");
        }

        public void Write(string fieldName, object value)
        {
            // Определение типа записываемого поля
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
            WinLogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        Timer Timer { get; set; }

        string Url { get; set; }

        void LoadData(bool firstStart)
        {
            var date = DateTime.Now;

            LogEvent("Срабатывание таймера в " + date.ToString("dd.MM.yyyy HH:mm:ss"), LogType.DETAILED);

            try
            {
                if (date.Minute == 3 || firstStart)
                {
                    string url = Url + "&interval=main" + "&t1=" + date.AddHours(-1).ToString("yyyyMMddHH0000.000") + "&t2=" + date.AddHours(-1).ToString("yyyyMMddHH3000.000");

                    try
                    {
                        string raw = GetHtml(url);

                        if (!string.IsNullOrEmpty(raw))
                        {
                            lock (Fields)
                            {
                                string[] rows = raw.Split('\n');

                                foreach (var key in Fields.Keys)
                                    if (key.Contains("Hour.")) Fields[key] = 0;

                                foreach (string row in rows)
                                {
                                    string[] parts = row.Split(',');

                                    if (parts.Length > 2)
                                    {
                                        string name = parts[0].Trim();
                                        float value = float.TryParse(parts[2].Trim().Replace(".", ","), out float f) ? f : 0;

                                        if (Fields.ContainsKey(name))
                                            Fields[name] = Convert.ToSingle(value) + value;
                                    }
                                }

                                Fields["Time"] = date.ToString("dd.MM.yyyy HH:mm:ss");
                            }
                        }
                        else
                        {
                            LogEvent("Нет часовых значений по ЭКОМ", LogType.ERROR);
                        }
                    }
                    catch (Exception e)
                    {
                        LogEvent("Ошибка при чтении часовых значений по ЭКОМ [" + e.Message + "]", LogType.ERROR);
                    }

                    if (date.Hour == 0 || firstStart)
                    {
                        date = date.AddDays(-1);
                        url = Url + "&interval=day" + "&t1=" + date.ToString("yyyyMMdd000000.000") + "&t2=" + date.ToString("yyyyMMdd235959.000");

                        try
                        {
                            string raw = GetHtml(url);

                            if (!string.IsNullOrEmpty(raw))
                            {
                                lock (Fields)
                                {
                                    string[] rows = raw.Split('\n');

                                    foreach (var key in Fields.Keys)
                                        if (key.Contains("Day.")) Fields[key] = 0;

                                    foreach (string row in rows)
                                    {
                                        string[] parts = row.Split(',');

                                        if (parts.Length > 2)
                                        {
                                            string name = parts[0].Trim();
                                            float value = float.TryParse(parts[2].Trim().Replace(".", ","), out float f) ? f : 0;

                                            if (Fields.ContainsKey(name)) Fields[name] = value;
                                        }
                                    }

                                    Fields["Time"] = date.ToString("dd.MM.yyyy HH:mm:ss");
                                }
                            }
                            else
                            {
                                LogEvent("Нет суточных значений по ЭКОМ", LogType.ERROR);
                            }
                        }
                        catch (Exception e)
                        {
                            LogEvent("Ошибка при суточных часовых значений по ЭКОМ [" + e.Message + "]", LogType.ERROR);
                        }
                    }
                }

                UpdateEvent();
            }
            catch (Exception e)
            {
                LogEvent("Ошибка при получении данных [" + e.Message + "]", LogType.ERROR);
            }
        }

        string GetHtml(string url)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.Timeout = 10000;
                req.Credentials = new NetworkCredential("modbus", "1");

                WebResponse res = req.GetResponse();
                StreamReader sr = new StreamReader(res.GetResponseStream(), Encoding.UTF8);

                string raw = sr.ReadToEnd();

                sr.Close();
                res.Close();

                Console.WriteLine("Url: " + url + "\nRaw: " + raw);

                return raw;
            }
            catch (Exception e)
            {
                LogEvent("Ошибка при HTTP запросе к источнику данных: " + e.Message, LogType.ERROR);
                return null;
            }
        }
    }
}