using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Drivers.INELT
{
	public class Driver : IDriver
    {
        public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

        public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

        public event LogEvent LogEvent;

        public event UpdateEvent UpdateEvent;

        public event WinLogEvent WinLogEvent;

        public bool Start(string jsonConfig)
        {
            LogEvent("Запуск ...");

            // сброс предыдущего состояния
            try { Port?.Close(); } catch { }
            try { Port = null; } catch { }

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

            // определение порта
            try
            {
                Port = new SerialPort
                {
                    PortName = Configuration.PortName,
                    BaudRate = Configuration.BaudRate,
                    DataBits = Configuration.DataBits,
                    StopBits = (StopBits)Configuration.StopBits,
                    Parity = (Parity)Configuration.Parity,
                };
				Port.DataReceived += Port_DataReceived;
            }
            catch (Exception e) {

                LogEvent("Ошибка при создании COM порта: " + e.Message, LogType.ERROR);
                return false;
            }

            // определение полей
            lock (Fields)
            {
                Fields["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 };
                Fields["ACTIVE"] = new DefField { Value = false, Quality = 0 };
                Fields["STATUS"] = new DefField { Value = "NOT CONNECTED", Quality = 0 };
                Fields["SELFTEST"] = new DefField { Value = "", Quality = 0 };
                Fields["LASTXFER"] = new DefField { Value = "", Quality = 0 };
                Fields["DATE"] = new DefField { Value = "", Quality = 0 };
                Fields["LASTSTEST"] = new DefField { Value = "", Quality = 0 };
                Fields["TONBATT"] = new DefField { Value = "", Quality = 0 };
                Fields["XOFFBATT"] = new DefField { Value = "", Quality = 0 };
                Fields["XONBATT"] = new DefField { Value = "", Quality = 0 };
                Fields["BCHARGE"] = new DefField { Value = "", Quality = 0 };
                Fields["LOADPCT"] = new DefField { Value = "", Quality = 0 };
                Fields["ITEMP"] = new DefField { Value = "", Quality = 0 };
                Fields["LINEV"] = new DefField { Value = "", Quality = 0 };
                Fields["OUTPUTV"] = new DefField { Value = "", Quality = 0 };
                Fields["TIMELEFT"] = new DefField { Value = "", Quality = 0 };
            }

            // запуск опроса
            Active = true;
            Thread = new Thread(ExchangeWithUPS);
            Thread.Start();

            // сигнализация о старте и первый опрос
            LogEvent("Мониторинг запущен");

            return true;
        }
		
		public void Stop()
        {
            LogEvent("Остановка ...");

            Active = false;
            try { Thread?.Abort(); } catch { }
            try { Thread = null; } catch { }
            try { Port?.Close(); } catch { }
            try { Port = null; } catch { }

            LogEvent("Мониторинг остановлен");
        }

        public void Write(string fieldName, object value)
        {
            // какая ещё запись в сраный инэлт
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
            WinLogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        SerialPort Port { get; set; }

        Thread Thread { get; set; }

        Values Values { get; set; }

        bool Active { get; set; } = false;

        bool Receive { get; set; } = false;

        void ExchangeWithUPS()
        {
            Values = new Values();

            int errorsCounter = 0;
            DateTime timeWorking;
            Values lastValues;

            while (Active)
            {
                timeWorking = DateTime.Now;
                lastValues = new Values();

                // проверка состояния порта
                if (!Port.IsOpen)
                {
                    try
                    {
                        Port.Open();
                    }
                    catch (Exception e)
                    {
                        LogEvent("COM-порт не открыт: " + e.Message, LogType.ERROR);
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
                    // Получение вольтажа, статуса, температуры и частоты Q1
                    try
                    {
                        Receive = false;

                        // отправка команды
                        Port.Write("Q1" + (char)0x0D);
                        LogEvent("Tx: Q1", LogType.DETAILED);
                        var txSendTime = DateTime.Now;

                        // ожидание ответа
                        Task.Delay(Configuration.RxTimeout).Wait();
                        if (!Active) break;

                        // разбор ответа
                        if (Receive)
                        {
                            byte[] answer = new byte[Port.BytesToRead];
                            Port.Read(answer, 0, answer.Length);
                            string text = Encoding.UTF8.GetString(answer);
                            LogEvent("Rx: " + text);

                            string[] tokens = text.Replace("(", "").Replace(".", ",").Split(new [] { " " }, StringSplitOptions.None);
                            lastValues.OUTPUTV = double.Parse(tokens[2]);
                            lastValues.ITEMP = double.Parse(tokens[6]);
                            lastValues.STATUS = "";
                            string[] statuses = new[] 
                            { 
                                "UTILITY FAIL",
                                "BATTERY LOW",
                                "BYPASS ACTIVE",
                                "UPS FAILED",
                                "STANDBY",
                                "TEST IN PROGRESS",
                                "SHUTDOWN ACTIVE",
                                "ONLINE"
                            };
                            for (var i = 0; i < 8; i++)
							{
                                if (tokens[7][i] == '1') lastValues.STATUS += statuses[i] + " ";
							}
                            lastValues.STATUS = lastValues.STATUS.Trim();
                            lastValues.DATE = DateTime.Now;
                        }
                        else
                        {
                            LogEvent("Rx: ничего не вернулось, таймаут " + Configuration.RxTimeout + " мс", LogType.DETAILED);
                            errorsCounter++;
                        }
                    }
                    catch (Exception e)
                    {
                        LogEvent("Ошибка при опросе значений: " + e.Message + "\n" + e.StackTrace, LogType.DETAILED);
                        errorsCounter++;
                    }

                    // Получение информации о батарее QBV
                    try
                    {
                        Receive = false;

                        // отправка команды
                        Port.Write("QBV" + (char)0x0D);
                        LogEvent("Tx: QBV", LogType.DETAILED);
                        var txSendTime = DateTime.Now;

                        // ожидание ответа
                        Task.Delay(Configuration.RxTimeout).Wait();
                        if (!Active) break;

                        // разбор ответа
                        if (Receive)
                        {
                            byte[] answer = new byte[Port.BytesToRead];
                            Port.Read(answer, 0, answer.Length);
                            string text = Encoding.UTF8.GetString(answer);
                            LogEvent("Rx: " + text);

                            string[] tokens = text.Replace("(", "").Replace(".", ",").Split(new[] { " " }, StringSplitOptions.None);
                            lastValues.BCHARGE = double.Parse(tokens[3]);
                            lastValues.TIMELEFT = double.Parse(tokens[4]);
                        }
                        else
                        {
                            LogEvent("Rx: ничего не вернулось, таймаут " + Configuration.RxTimeout + " мс", LogType.DETAILED);
                            errorsCounter++;
                        }
                    }
                    catch (Exception e)
                    {
                        LogEvent("Ошибка при опросе значений: " + e.Message + "\n" + e.StackTrace, LogType.DETAILED);
                        errorsCounter++;
                    }

                    // Получение информации о разряде батареи QGS
                    try
                    {
                        Receive = false;

                        // отправка команды
                        Port.Write("QGS" + (char)0x0D);
                        LogEvent("Tx: QGS", LogType.DETAILED);
                        var txSendTime = DateTime.Now;

                        // ожидание ответа
                        Task.Delay(Configuration.RxTimeout).Wait();
                        if (!Active) break;

                        // разбор ответа
                        if (Receive)
                        {
                            byte[] answer = new byte[Port.BytesToRead];
                            Port.Read(answer, 0, answer.Length);
                            string text = Encoding.UTF8.GetString(answer);
                            LogEvent("Rx: " + text);

                            string[] tokens = text.Replace("(", "").Replace(".", ",").Split(new[] { " " }, StringSplitOptions.None);
                            lastValues.LINEV = double.Parse(tokens[0]);
                            lastValues.LOADPCT = double.Parse(tokens[5]);
                        }
                        else
                        {
                            LogEvent("Rx: ничего не вернулось, таймаут " + Configuration.RxTimeout + " мс", LogType.DETAILED);
                            errorsCounter++;
                        }
                    }
                    catch (Exception e)
                    {
                        LogEvent("Ошибка при опросе значений: " + e.Message + "\n" + e.StackTrace, LogType.DETAILED);
                        errorsCounter++;
                    }
                

                    // Определение изменений в состоянии ИБП
                    if (lastValues.STATUS != Values.STATUS)
                    {
                        Log("Статус изменился с [" + Values.STATUS + "] на [" + lastValues.STATUS + "]");
                    }
                    lastValues.ACTIVE = lastValues.STATUS.Contains("ONLINE");

                    if (lastValues.LINEV < 200)
                    {
                        if (lastValues.LINEV != Values.LINEV)
                        {
                            Log("Напряжение меньше 200 V");
                        }
                    }
                    else if (Values.LINEV < 200)
                    {
                        Log("Напряжение в норме");
                    }

                    if (lastValues.ITEMP > 40)
                    {
                        if (lastValues.ITEMP != Values.ITEMP)
                        {
                            Log("Температура больше 40 С");
                        }
                    }
                    else if (Values.ITEMP > 40)
                    {
                        Log("Температура в норме");
                    }

                    if (lastValues.TIMELEFT < 30)
                    {
                        if (lastValues.TIMELEFT != Values.TIMELEFT)
                        {
                            Log("Время до отключения меньше 30 минут");
                        }
                    }
                    else if (Values.TIMELEFT < 30)
                    {
                        Log("Время до отключения в норме");
                    }
                }
                else
                {
                    lastValues.ACTIVE = false;
                }

                if (lastValues.ACTIVE != Values.ACTIVE)
                {
                    if (lastValues.ACTIVE)
                    {
                        Log("Восстановлена связь со службой ApcUpsD");
                    }
                    else
                    {
                        Log("Служба ApcUpsD не отвечает");
                    }
                }

                Values = lastValues;

                // Передача полученных значений в OPC
                lock (Fields)
                {
                    Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss"); 
                    Fields["ACTIVE"].Value = lastValues.ACTIVE;
                    Fields["STATUS"].Value = lastValues.STATUS;
                    Fields["SELFTEST"].Value = lastValues.SELFTEST;
                    Fields["LASTXFER"].Value = lastValues.LASTXFER;
                    Fields["DATE"].Value = lastValues.DATE;
                    Fields["LASTSTEST"].Value = lastValues.LASTSTEST;
                    Fields["TONBATT"].Value = lastValues.TONBATT;
                    Fields["XOFFBATT"].Value = lastValues.XOFFBATT;
                    Fields["XONBATT"].Value = lastValues.XONBATT;
                    Fields["BCHARGE"].Value = lastValues.BCHARGE;
                    Fields["LOADPCT"].Value = lastValues.LOADPCT;
                    Fields["ITEMP"].Value = lastValues.ITEMP;
                    Fields["LINEV"].Value = lastValues.LINEV;
                    Fields["OUTPUTV"].Value = lastValues.OUTPUTV;
                    Fields["TIMELEFT"].Value = lastValues.TIMELEFT;
                    foreach (var value in Fields.Values)
					{
                        value.Quality = 192;
					}
                }
                UpdateEvent();

                // ожидание, когда же это наконец закончится
                if (errorsCounter > 20)
                {
                    Port.Close();
                    errorsCounter = 0;
                    Thread.Sleep(1000);
                } 
                else
				{
                    int timeout = Convert.ToInt32(Configuration.Timeout - (DateTime.Now - timeWorking).TotalMilliseconds);
                    if (timeout > 0) Thread.Sleep(timeout);
                }
            }
        }
        void Log(string message)
        {
            LogEvent(message);
            EventLog.WriteEntry("apcupsd", "UPS \"" + Configuration.Name + "\": " + message, EventLogEntryType.Error);
        }

        void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Receive = true;
        }
    }
}