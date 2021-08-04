using ISTOK.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.ISTOK
{
    public class Driver : IDriver
    {
        public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

        public event LogEvent LogEvent;

        public event UpdateEvent UpdateEvent;

        public event WinLogEvent WinLogEvent;

        public bool Start(string jsonConfig)
        {
            LogEvent("Запуск ...");
            BuildFields();

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

            // Проверка параметров
            try
            {
                ComPort = new ComPort
                {
                    Number = Configuration.Number,
                    BaudRate = Configuration.BaudRate,
                    Parity = Configuration.Parity,
                    StopBits = Configuration.StopBits,
                    Timeout = Configuration.ComTimeout,
                };
            }
            catch (Exception e)
            {
                return Err("Ошибка при установке параметров COM порта: " + e.Message);
            }

            // Запуск потока, опрашивающего прибор
            if (Thread != null)
            {
                Thread.Abort();
                Thread = null;
            }

            Thread = new Thread(ThreadTick) { IsBackground = true };

            ExchangeActive = true;
            Thread.Start();

            LogEvent("Мониторинг запущен");

            return true;
        }

        public void Stop()
        {
            ExchangeActive = false;
        }

        public void Write(string fieldName, object value)
        {
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
            WinLogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        DateTime ExchangeStartDate { get; set; }

        Thread Thread { get; set; }

        ComPort ComPort { get; set; }

        bool ExchangeActive { get; set; } = false;

        int CurrentHour { get; set; }

        int CurrentDay { get; set; }

        void BuildFields()
        {
            Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss") });

            var parameters = new string[] { "Q", "G", "H", "dP", "T", "P", "V", "R" };

            Fields.Add("Current.ColdSource.T", new DefField { Value = 0F });
            Fields.Add("Current.ColdSource.P", new DefField { Value = 0F });
            Fields.Add("Current.ColdSource.Patm", new DefField { Value = 0F });
            Fields.Add("Current.ColdSource.H", new DefField { Value = 0F });

            for (byte k = 0; k < 4; k++)
                for (byte i = 0; i < parameters.Length; i++)
                    Fields.Add("Current.Points." + k + "." + parameters[i], new DefField { Value = 0F });

            for (byte k = 0; k < 11; k++)
                Fields.Add("Current.Channels." + k, new DefField { Value = 0F });

            for (byte k = 0; k < 4; k++)
                Fields.Add("Current.Groups." + k, new DefField { Value = 0F });


            // часовые

            parameters = new string[] { "T", "P", "Patm", "W", "G", "M" };

            for (byte k = 0; k < 4; k++)
                for (byte i = 0; i < parameters.Length; i++)
                    Fields.Add("Hour." + k + "." + parameters[i], new DefField { Value = 0F });


            // суточные

            for (byte k = 0; k < 4; k++)
                for (byte i = 0; i < parameters.Length; i++)
                    Fields.Add("Day." + k + "." + parameters[i], new DefField { Value = 0F });
        }

        void ThreadTick()
        {
            while (ExchangeActive)
            {
                ExchangeStartDate = DateTime.Now;

                // Проверка доступности COM-порта

                bool comReady = true;

                try
                {
                    if (Istok.GetComFuncError() != 0)
                    {
                        Istok.FreeComPort();
                        Istok.SetComPortConfig(ComPort, true);
                        Istok.InitComPort();
                    }
                }
                catch (Exception e)
                {
                    comReady = Err("Ошибка при подключении к COM-порту: " + e.Message);
                }

                try
                {
                    Istok.ReadDateTime(1, out DateTime deviceDate);
                }
                catch (Exception e)
                {
                    comReady = Err("Ошибка при подключении к COM-порту: " + e.Message);
                }


                // Получение данных

                if (comReady)
                {
                    lock (Fields)
                    {
                        if (ExchangeStartDate.Hour != CurrentHour)
                        {
                            CurrentHour = ExchangeStartDate.Hour;
                            ReadHourValues();
                        }

                        if (ExchangeStartDate.Day != CurrentDay)
                        {
                            CurrentDay = ExchangeStartDate.Day;
                            ReadDayValues();
                        }

                        ReadCurrentValues();

                        Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
                    }

                    UpdateEvent();
                }

                // Ожидание следующего срабатывания

                int timeout = Convert.ToInt32(Configuration.Timeout - (DateTime.Now - ExchangeStartDate).TotalMilliseconds);
                if (timeout > 0) Thread.Sleep(timeout);
            }

            Istok.FreeComPort();
        }

        void ReadCurrentValues()
        {
            Istok.ReadOperativeData(1, out Operative data);
        }

        void ReadHourValues()
        {
            Istok.ReadHourData(1, 1, DateTime.Now, out Archive data);
            Istok.ReadHourData(1, 2, DateTime.Now, out data);
            Istok.ReadHourData(1, 3, DateTime.Now, out data);
            Istok.ReadHourData(1, 4, DateTime.Now, out data);
        }

        void ReadDayValues()
        {
            Istok.ReadDayData(1, 1, DateTime.Now, out Archive data);
            Istok.ReadDayData(1, 2, DateTime.Now, out data);
            Istok.ReadDayData(1, 3, DateTime.Now, out data);
            Istok.ReadDayData(1, 4, DateTime.Now, out data);
        }

        bool Err(string text)
        {
            LogEvent(text, LogType.ERROR);
            return false;
        }
    }
}