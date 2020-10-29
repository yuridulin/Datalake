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
				//.Replace("seconds", "")
				.Replace("+0300", "")
				.Replace("(чшьр)", "")
				.Replace("  ", " ")
				.Trim();

			if (DateTime.TryParse(text, out DateTime dt)) return dt;
            else if (DateTime.TryParseExact(text, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out dt)) return dt;
            else if (DateTime.TryParseExact(text, "ddd MMM dd HH:mm:ss yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return dt;
            else if (DateTime.TryParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return dt;
            else if (DateTime.TryParseExact(text, "MM/dd/yy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return dt;
            else return new DateTime(1, 1, 1);
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
                WinLogEvent("Перезапуск процесса ApcUpsD \"" + Configuration.Exe + "\"");
            }

            // Создаем подключение к серверу данных и читаем вывод из консоли
            string raw = "";
            try
            {
                using (var cmd = new Process())
                {
                    cmd.StartInfo = new ProcessStartInfo
                    {
                        FileName = Environment.CurrentDirectory + @"\Drivers\ApcAccess\ApcAccess.exe",
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
                if (stats.ITEMP != Stats.ITEMP)
                {
                    Log("Время до отключения меньше 30 минут");
                }
            }
            else if (Stats.ITEMP < 30)
            {
                Log("Время до отключения в норме");
            }

            // Обновление OPC полей
            lock (Fields)
            {
                Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
                Fields["ACTIVE"] = stats.ACTIVE;
                Fields["STATUS"] = stats.STATUS;
                Fields["SELFTEST"] = stats.SELFTEST;
                Fields["LASTXFER"] = stats.LASTXFER;
                Fields["DATE"] = stats.DATE.ToString("dd.MM.yyyy HH:mm:ss");
                Fields["LASTSTEST"] = stats.LASTSTEST;
                Fields["TONBATT"] = stats.TONBATT;
                Fields["XOFFBATT"] = stats.XOFFBATT;
                Fields["XONBATT"] = stats.XONBATT;
                Fields["BCHARGE"] = stats.BCHARGE;
                Fields["LOADPCT"] = stats.LOADPCT;
                Fields["ITEMP"] = stats.ITEMP;
                Fields["LINEV"] = stats.LINEV;
                Fields["OUTPUTV"] = stats.OUTPUTV;
                Fields["TIMELEFT"] = stats.TIMELEFT;
            }

            UpdateEvent();
            Stats = stats;
        }

        void Log(string message)
		{
            LogEvent(message);
            EventLog.WriteEntry("apcupds", "UPS \"" + Configuration.Name + "\": " + message);
        }

        void LoadError()
		{
            Fields["ACTIVE"] = Stats.ACTIVE = false;
            Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
            UpdateEvent();
        }
    }
}