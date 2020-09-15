using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.INELT
{
    public class Driver : IDriver
    {
        public Dictionary<string, object> Fields { get; set; } = new Dictionary<string, object>();

        public event LogEvent LogEvent;

        public event UpdateEvent UpdateEvent;

        public event WinLogEvent WinLogEvent;

        public bool Start(string jsonConfig)
        {
            LogEvent("Запуск ...");

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

            Fields = new Dictionary<string, object>
            {
                { "Time", DateTime.Now.ToString("HH:mm:ss") },
                { "APC", "" },
                { "HOSTNAME", "" },
                { "RELEASE", "" },
                { "VERSION", "" },
                { "UPSNAME", "" },
                { "CABLE", "" },
                { "MODEL", "" },
                { "UPSMODE", "" },
                { "STARTTIME", "" },
                { "STATUS", "" },
                { "MBATTCHG", "" },
                { "MINTIMEL", "" },
                { "MAXTIME", "" },
                { "MAXLINEV", "" },
                { "MINLINEV", "" },
                { "SENSE", "" },
                { "DWAKE", "" },
                { "DSHUTD", "" },
                { "DLOWBATT", "" },
                { "LOTRANS", "" },
                { "HITRANS", "" },
                { "RETPCT", "" },
                { "ALARMDEL", "" },
                { "LINEFREQ", "" },
                { "LASTXFER", "" },
                { "NUMXFERS", "" },
                { "TONBATT", "" },
                { "CUMONBATT", "" },
                { "XOFFBATT", "" },
                { "SELFTEST", "" },
                { "STESTI", "" },
                { "STATFLAG", "" },
                { "DIPSW", "" },
                { "REG1", "" },
                { "REG2", "" },
                { "REG3", "" },
                { "SERIALNO", "" },
                { "NOMOUTV", "" },
                { "NOMBATTV", "" },
                { "EXTBATTS", "" },
                { "BADBATTS", "" },
                { "FIRMWARE", "" },
                { "APCMODEL", "" },
                { "CURRENT", "" },
                { "LOWRANS", "" },
                { "DRIVER", "" },
                { "XONBATT", "" },
                { "NOMPOWER", "" },
                { "LASTSTEST", "" },

                { "LINEV", 0F },
                { "LOADPCT", 0F },
                { "BCHARGE", 0F },
                { "TIMELEFT", 0F },
                { "OUTPUTV", 0F },
                { "ITEMP", 0F },
                { "BATTV", 0F },

                { "DATE", DateTime.Now },
                { "MANDATE", DateTime.Now },
                { "BATTDATE", DateTime.Now },
                { "END APC", DateTime.Now },
            };

            Timer = new Timer(Configuration.Timeout);
            Timer.Elapsed += (s, e) => LoadData();
            Timer.Start();

            LogEvent("Мониторинг запущен");

            Task.Run(LoadData);

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
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
            WinLogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        Timer Timer { get; set; }

        string[] NumberFields { get; set; } = new string[] { "LINEV", "LOADPCT", "BCHARGE", "TIMELEFT", "OUTPUTV", "ITEMP", "BATTV", };

        string[] DateTimeFields { get; set; } = new string[] { "DATE", "MANDATE", "BATTDATE", "END APC", };

        void LoadData()
        {
            var lines = new string[0];
            var pairs = new Dictionary<string, object>();

            try
            {
                string raw = "";

                try
                {
                    raw = File.ReadAllText(Configuration.Url);
                }
                catch (Exception)
                {
                    LogEvent("Источник данных не отвечает", LogType.ERROR);
                    return;
                }

                if (raw == "")
                {
                    LogEvent("Источник данных не предоставляет данных для обработки", LogType.ERROR);
                    return;
                }

                lines = raw.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception e)
            {
                LogEvent("Ошибка при разделении линий [" + e.Message + "]", LogType.ERROR);
                return;
            }

            try
            {
                lock (Fields)
                {
                    foreach (string line in lines)
                    {
                        if (line.Contains(":"))
                        {
                            int i = line.IndexOf(':');
                            string name = line.Substring(0, i - 1).Trim().ToUpper();
                            string val = line.Substring(i + 1).Trim();

                            // Конвертация значений по типам
                            object value = string.IsNullOrEmpty(val) ? null : val;

                            if (NumberFields.Contains(name))
                            {
                                string[] vs = val.Split(' ');
                                foreach (string s in vs)
                                {
                                    if (float.TryParse(s.Replace(".", ","), out float f))
                                    {
                                        Fields[name] = f.ToString().Replace(',', '.');
                                        break;
                                    }
                                }
                            }

                            else if (DateTimeFields.Contains(name))
                            {
                                val = val
                                    .Replace("╠шэёъюх тЁхь", "")
                                    .Replace("Минское время", "")
                                    .Replace("Калининградское время (зима)", "")
                                    .Replace("+0300", "")
                                    .Trim();

                                if (DateTime.TryParseExact(val, "ddd MMM dd HH:mm:ss  yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime d))
                                {
                                    Fields[name] = d.ToString("dd.MM.yyyy HH:mm:ss");
                                }
                                if (DateTime.TryParseExact(val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                                {
                                    Fields[name] = d.ToString("dd.MM.yyyy HH:mm:ss");
                                }
                            }

                            else if (Fields.ContainsKey(name))
                            {
                                Fields[name] = value;
                            }
                        }
                    }

                    Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
                }

                UpdateEvent();
            }
            catch (Exception)
            {
                LogEvent("Ошибка при конвертации значения", LogType.ERROR);
                return;
            }

            
        }
    }
}