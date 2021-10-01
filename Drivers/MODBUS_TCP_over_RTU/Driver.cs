using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace iNOPC.Drivers.PR20_RS485
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

            try { Port?.Close(); } catch (Exception) { }
            Port = new SerialPort();
            Port.DataReceived += (s, e) => Receive = true;
            Port.ErrorReceived += (s, e) => ComError(e.EventType.ToString());

            // чтение конфигурации
            try
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
            }
            catch (Exception e)
            {
                return Err("Конфигурация не прочитана: " + e.Message);
            }

            if (Configuration.Fields.Count == 0)
            {
                return Err("Список опрашиваемых полей пуст");
            }
            if (!Fields.ContainsKey("Time"))
            {
                Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 });
            }

            UpdateEvent();

            // установка начальных значений
            try
            {
                Port.PortName = Configuration.PortName;
                Port.BaudRate = Configuration.BaudRate;
                Port.DataBits = Configuration.DataBits;
                Port.Parity = (Parity)Configuration.Parity;
                Port.StopBits = (StopBits)Configuration.StopBits;
                Port.ReadTimeout = 1000;
                Port.WriteTimeout = 1000;
            }
            catch (Exception e)
            {
                return Err("Параметры COM порта не установлены: " + e.Message + "\n" + e.StackTrace);
            }

            Active = true;

            if (Thread != null)
            {
                Thread.Abort();
                Thread = null;
            }

            Thread = new Thread(TrancievePackages);
            Thread.Start();

            LogEvent("Мониторинг запущен");

            return true;
        }

        public void Stop()
        {
            LogEvent("Остановка ...");

            Active = false;
            try { Port.Close(); } catch (Exception) { }

            LogEvent("Мониторинг остановлен");
        }

        public void Write(string fieldName, object value)
        {
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
            WinLogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        SerialPort Port { get; set; }

        Thread Thread { get; set; }

        DateTime Date { get; set; }

        DateTime PackageDate { get; set; }

        bool Active { get; set; } = false;

        bool Receive { get; set; } = false;

        void TrancievePackages()
        {
            int errorsCounter = 0;

            while (Active)
            {
                Date = DateTime.Now;

                if (!Port.IsOpen)
                {
                    try
                    {
                        Port.Open();
                    }
                    catch (Exception e)
                    {
                        Err("COM-порт не открыт: " + e.Message + "\n" + e.StackTrace);
                        try
                        {
                            Port.Close();
                            Thread.Sleep(1000);
                        }
                        catch (Exception) { }
                    }
                }

                if (Port.IsOpen)
                {
                    // запросы к прибору
                    foreach (var field in Configuration.Fields)
                    {
                        if (!Active) break;
                        try
                        {
                            Receive = false;
                            PackageDate = DateTime.Now;

                            byte[] command = field.Command();
                            Port.Write(command, 0, command.Length);
                            LogEvent("Tx: " + Helpers.BytesToString(command), LogType.DETAILED);

                            while (!Receive && (DateTime.Now - PackageDate).TotalMilliseconds < 5000)
                            {
                                Thread.Sleep(1);
                            }

                            if (Receive)
                            {
                                byte[] answer = new byte[field.Length()];
                                Port.Read(answer, 0, field.Length());
                                LogEvent("Rx: " + Helpers.BytesToString(answer));
                            }
                            else
                            {
                                LogEvent("Rx: ничего не вернулось, таймаут " + 5000 + " мс", LogType.DETAILED);
                                errorsCounter++;
                            }
                        }
                        catch (Exception e)
                        {
                            LogEvent("Ошибка при опросе значений: " + e.Message + "\n" + e.StackTrace, LogType.DETAILED);
                            errorsCounter++;
                        }
                    }
                }

                lock (Fields)
                {
                    Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
                }
                UpdateEvent();

                int timeout = Convert.ToInt32(Configuration.CyclicTimeout - (DateTime.Now - Date).TotalMilliseconds);
                if (timeout > 0) Thread.Sleep(timeout);

                if (errorsCounter > 20)
                {
                    Port.Close();
                    errorsCounter = 0;
                    Thread.Sleep(1000);
                }
            }

            try
            {
                Port.Close();
            }
            catch (Exception) { }
        }

        void ComError(string errorName)
        {
            LogEvent("COM: " + errorName, LogType.DETAILED);
            Port.BaseStream.Flush();
            Port.DiscardInBuffer();
            Port.DiscardOutBuffer();
        }

        bool Err(string text)
        {
            LogEvent(text, LogType.ERROR);
            return false;
        }
    }
}
