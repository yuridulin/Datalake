using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using iNOPC.Drivers.APC_UPS;
using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.APC
{
    public class Driver : IDriver
    {
        public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

        public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

        public event LogEvent LogEvent;

        public event UpdateEvent UpdateEvent;

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

            // Определение имени процесс ApcUpsD
            try
			{
                ExeName = Configuration.Exe.Substring(Configuration.Exe.LastIndexOf('\\') + 1).Replace(".exe", "");
            }
            catch (Exception e)
            {
                LogEvent("Не удалось определить имя процесса ApcUpsD по строке \"" + Configuration.Exe + "\": " + e.Message, LogType.ERROR);
                return false;
            }

            // Определение полей
            var stats = new Stats();
            Fields["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 };
            Fields["ACTIVE"] = new DefField { Value = stats.ACTIVE, Quality = 0 };
            Fields["STATUS"] = new DefField { Value = stats.STATUS, Quality = 0 };
            Fields["SELFTEST"] = new DefField { Value = stats.SELFTEST, Quality = 0 };
            Fields["LASTXFER"] = new DefField { Value = stats.LASTXFER, Quality = 0 };
            Fields["DATE"] = new DefField { Value = stats.DATE.ToString("dd.MM.yyyy HH:mm:ss"), Quality = 0 };
            Fields["LASTSTEST"] = new DefField { Value = stats.LASTSTEST, Quality = 0 };
            Fields["TONBATT"] = new DefField { Value = stats.TONBATT, Quality = 0 };
            Fields["XOFFBATT"] = new DefField { Value = stats.XOFFBATT, Quality = 0 };
            Fields["XONBATT"] = new DefField { Value = stats.XONBATT, Quality = 0 };
            Fields["BCHARGE"] = new DefField { Value = stats.BCHARGE, Quality = 0 };
            Fields["LOADPCT"] = new DefField { Value = stats.LOADPCT, Quality = 0 };
            Fields["ITEMP"] = new DefField { Value = stats.ITEMP, Quality = 0 };
            Fields["LINEV"] = new DefField { Value = stats.LINEV, Quality = 0 };
            Fields["OUTPUTV"] = new DefField { Value = stats.OUTPUTV, Quality = 0 };
            Fields["TIMELEFT"] = new DefField { Value = stats.TIMELEFT, Quality = 0 };

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
            //WinLogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        Timer Timer { get; set; }

        Stats Stats { get; set; } = new Stats { ACTIVE = false, DATE = DateTime.Today };

        string ExeName { get; set; }


        DateTime ToDate(string text)
		{
			text = text
				.Replace("╠шэёъюх тЁхь", "")
				.Replace("Минское время", "")
				.Replace("Калининградское время", "")
				.Replace("(зима)", "")
				.Replace("RTZ 2", "")
				.Replace("Seconds", "")
				.Replace("+0300", "")
				.Replace("(чшьр)", "")
				.Replace("  ", " ")
				.Replace("  ", " ")
				.Replace("  ", " ")
                .Trim();

            if (DateTime.TryParse(text, out DateTime dt)) return dt;
            else if (DateTime.TryParseExact(text, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return dt;
            else if (DateTime.TryParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return dt;
            else if (DateTime.TryParseExact(text, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return dt;
            else
            {
                LogEvent("Дата не распознана: " + text, LogType.WARNING);
                return new DateTime(1, 1, 1);
            }
        }

        double ToNumeric(string text)
		{
            string[] vs = text.Split(' ');
            foreach (string s in vs)
            {
                if (double.TryParse(s.Replace(".", ","), out double d))
                {
                    return d;
                }
            }

            return 0;
        }

        void LoadData()
        {
            // Проверка, запущен ли процесс ApcUpsD. Если нет - процесс запускается по указанному в конфиге пути
            var processes = Process.GetProcesses().Where(x => x.ProcessName == ExeName);
            if (processes.Count() < 1)
            {
                Process.Start(Configuration.Exe);
                LogEvent("Перезапуск процесса ApcUpsD \"" + Configuration.Exe + "\"", LogType.ERROR);
                //WinLogEvent("Перезапуск процесса ApcUpsD \"" + Configuration.Exe + "\"");
            }

            // Создаем подключение к серверу данных и читаем вывод из консоли
            string raw = "";
            try
            {
                using (var cmd = new Process())
                {
                    cmd.StartInfo = new ProcessStartInfo
                    {
                        FileName = @"C:\iNOPC\Drivers\ApcAccess\ApcAccess.exe",
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
                LoadError();
                return;
            }

            if (raw == "")
            {
                LogEvent("Источник данных не предоставляет данных для обработки", LogType.ERROR);
                LoadError();
                return;
            }

            // обрабатываем полученный вывод, деля построчно
            var lines = new string[0];
            try
            {
                lines = raw.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception e)
            {
                LogEvent("Ошибка при разделении строк [" + e.Message + "]", LogType.ERROR);
                LoadError();
                return;
            }

            var stats = new Stats();

            foreach (string line in lines)
            {
                if (line.Contains(":"))
                {
                    try
                    {
                        // разбиваем строку на пару "ключ" - "значение"
                        int i = line.IndexOf(':');
                        string name = line.Substring(0, i - 1).Trim().ToUpper();
                        string val = line.Substring(i + 1).Trim();

                        // получаем нужные значения
                        switch (name)
                        {
                            case "STATUS":
                                stats.STATUS = val;
                                break;

                            case "SELFTEST":
                                stats.SELFTEST = val;
                                break;

                            case "LASTXFER":
                                stats.LASTXFER = val;
                                break;

                            case "DATE":
                                stats.DATE = ToDate(val);
                                break;

                            case "LASTSTEST":
                                stats.LASTSTEST = val;
                                break;

                            case "TONBATT":
                                stats.TONBATT = val;
                                break;

                            case "XONBATT":
                                stats.XONBATT = val;
                                break;

                            case "XOFFBATT":
                                stats.XOFFBATT = val;
                                break;

                            case "BCHARGE":
                                stats.BCHARGE = ToNumeric(val);
                                break;

                            case "LOADPCT":
                                stats.LOADPCT = ToNumeric(val);
                                break;

                            case "ITEMP":
                                stats.ITEMP = ToNumeric(val);
                                break;

                            case "LINEV":
                                stats.LINEV = ToNumeric(val);
                                break;

                            case "OUTPUTV":
                                stats.OUTPUTV = ToNumeric(val);
                                break;

                            case "TIMELEFT":
                                stats.TIMELEFT = ToNumeric(val);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        LogEvent("Строка не разобрана: " + line + "\n" + e.Message + "\n" + e.StackTrace, LogType.WARNING);
                    }
                }
            }

            stats.ACTIVE = (DateTime.Now - stats.DATE).TotalMinutes < 2;

            // Проверка уставок по значимым полям
            if (stats.STATUS != Stats.STATUS)
			{
                Log("Статус изменился с [" + Stats.STATUS + "] на [" + stats.STATUS + "]");
			}

            if (stats.ACTIVE != Stats.ACTIVE)
			{
                if (stats.ACTIVE)
				{
                    Log("Восстановлена связь со службой ApcUpsD");
                }
                else
				{
                    Log("Служба ApcUpsD не отвечает");
                }
            }

            if (stats.LINEV < 200)
			{
                if (stats.LINEV != Stats.LINEV)
				{
                    Log("Напряжение меньше 200 V");
                }
			} 
            else if (Stats.LINEV < 200)
            {
                Log("Напряжение в норме");
            }

            if (stats.ITEMP > 40)
            {
                if (stats.ITEMP != Stats.ITEMP)
                {
                    Log("Температура больше 40 С");
                }
            }
            else if (Stats.ITEMP > 40)
            {
                Log("Температура в норме");
            }

            if (stats.TIMELEFT < 30)
            {
                if (stats.TIMELEFT != Stats.TIMELEFT)
                {
                    Log("Время до отключения меньше 30 минут");
                }
            }
            else if (Stats.TIMELEFT < 30)
            {
                Log("Время до отключения в норме");
            }

            // Обновление OPC полей
            lock (Fields)
            {
                Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
                Fields["ACTIVE"].Value = stats.ACTIVE;
                Fields["STATUS"].Value = stats.STATUS;
                Fields["SELFTEST"].Value = stats.SELFTEST;
                Fields["LASTXFER"].Value = stats.LASTXFER;
                Fields["DATE"].Value = stats.DATE.ToString("dd.MM.yyyy HH:mm:ss");
                Fields["LASTSTEST"].Value = stats.LASTSTEST;
                Fields["TONBATT"].Value = stats.TONBATT;
                Fields["XOFFBATT"].Value = stats.XOFFBATT;
                Fields["XONBATT"].Value = stats.XONBATT;
                Fields["BCHARGE"].Value = stats.BCHARGE;
                Fields["LOADPCT"].Value = stats.LOADPCT;
                Fields["ITEMP"].Value = stats.ITEMP;
                Fields["LINEV"].Value = stats.LINEV;
                Fields["OUTPUTV"].Value = stats.OUTPUTV;
                Fields["TIMELEFT"].Value = stats.TIMELEFT;
                foreach (var key in Fields.Keys) Fields[key].Quality = 192;
            }

            UpdateEvent();
            Stats = stats;
        }

        void Log(string message)
		{
            LogEvent(message);
            EventLog.WriteEntry("apcupsd", "UPS \"" + Configuration.Name + "\": " + message, EventLogEntryType.Error);
        }

        void LoadError()
		{
            Fields["ACTIVE"].Value = Stats.ACTIVE = false;
            Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
            UpdateEvent();
        }
    }
}