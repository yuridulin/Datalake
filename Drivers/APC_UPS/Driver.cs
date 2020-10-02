using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.APC_UPS
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
            Fields = new Dictionary<string, object>
            {
                { "Time", "" },
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

                { "LINEV", 0 },
                { "LOADPCT", 0 },
                { "BCHARGE", 0 },
                { "TIMELEFT", 0 },
                { "OUTPUTV", 0 },
                { "ITEMP", 0 },
                { "BATTV", 0 },

                { "DATE", DateTime.Now },
                { "MANDATE", DateTime.Now },
                { "BATTDATE", DateTime.Now },
                { "END APC", DateTime.Now },
            };

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
            // Определение типа записываемого поля
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
            WinLogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        Timer Timer { get; set; }

        private string[] DateTimeFields { get; set; } = new string[] { "DATE", "XONBATT", "TONBATT", "XOFFBATT", "LASTSTEST", "END APC", "MANDATE", "BATTDATE" };

        private string[] NumberFields { get; set; } = new string[] { "LINEV", "LOADPCT", "BCHARGE", "TIMELEFT", "OUTPUTV", "ITEMP", "BATTV" };

        void LoadData()
        {
            var lines = new string[0];

            // Создаем подключение к серверу данных и читаем вывод из консоли
            string raw = "";
            try
            {
                using (var cmd = new Process())
                {
                    cmd.StartInfo = new ProcessStartInfo
                    {
                        FileName = Configuration.Path,
                        Arguments = "status " + Configuration.Url,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    cmd.Start();
                    raw = cmd.StandardOutput.ReadToEnd();
                }
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


            // обрабатываем полученный вывод, деля построчно
            try
            {
                lines = raw.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception e)
            {
                LogEvent("Ошибка при разделении строк [" + e.Message + "]", LogType.ERROR);
                return;
            }

            lock (Fields)
            {
                foreach (string line in lines)
                {
                    // каждую строку разбиваем на пару "ключ" - "значение"
                    if (line.Contains(":"))
                    {
                        try
                        {
                            int i = line.IndexOf(':');
                            string name = line.Substring(0, i - 1).Trim().ToUpper();
                            string val = line.Substring(i + 1).Trim();

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
                                    .Replace("Калининградское время", "")
                                    .Replace("(зима)", "")
                                    .Replace("RTZ 2", "")
                                    .Replace("Seconds", "")
                                    //.Replace("seconds", "")
                                    .Replace("+0300", "")
                                    .Replace("(чшьр)", "")
                                    .Replace("  ", " ")
                                    .Trim();

                                //Thu Oct 01 10:54:45 2020
                                if (DateTime.TryParse(val, out DateTime d))
                                {
                                    Fields[name] = d.ToString("dd.MM.yyyy HH:mm:ss");
                                }
                                else if (DateTime.TryParseExact(val, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out d))
                                {
                                    Fields[name] = d.ToString("dd.MM.yyyy HH:mm:ss");
                                }
                                else if (DateTime.TryParseExact(val, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                                {
                                    Fields[name] = d.ToString("dd.MM.yyyy HH:mm:ss");
                                }
                                else if (DateTime.TryParseExact(val, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                                {
                                    Fields[name] = d.ToString("dd.MM.yyyy HH:mm:ss");
                                }
                                else if (DateTime.TryParseExact(val, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                                {
                                    Fields[name] = d.ToString("dd.MM.yyyy HH:mm:ss");
                                }
                                else
								{
                                    LogEvent("Строка с датой не разобрана: " + val, LogType.WARNING);
                                    Fields[name] = val;
                                }
                            }

                            else if (Fields.ContainsKey(name))
                            {
                                Fields[name] = string.IsNullOrEmpty(val) ? null : val;
                            }
                        }
                        catch (Exception e)
                        {
                            LogEvent("Строка не разобрана: " + line + "\n" + e.Message + "\n" + e.StackTrace, LogType.WARNING);
                        }
                    }
                }

                Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
            }
            
            UpdateEvent();
        }
    }
}