using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace iNOPC.Drivers.ENERGOMERA_CE303
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

            // удаление хвостов
            try { Port?.Close(); } catch { }
            try { Thread?.Abort(); } catch { }

            // чтение конфигурации
            try
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
            }
            catch (Exception e)
            {
                return Err("Конфигурация не прочитана: " + e.Message);
            }

            // определение параметров
            Devices = Configuration.Devices.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            if (Devices.Length == 0) return Err("В конфигурации не определены конечные устройства");

            Initialize();

            // подключение
            try
            {
                Port = new SerialPort
                {
                    PortName = Configuration.PortName,
                    BaudRate = 9600,
                    Parity = Parity.Even,
                    DataBits = 7,
                    StopBits = StopBits.One,
                };

                Port.ErrorReceived += (s, e) => ComError(e.EventType.ToString());
            }
            catch (Exception e)
            {
                return Err("Ошибка при настройке COM порта: " + e.Message + "\n" + e.StackTrace);
            }

            // запуск опросов
            Active = true;

            Thread = new Thread(Monitoring);
            Thread.Start();

            // сигнализация о завершении
            LogEvent("Мониторинг запущен");

            return true;
        }

        public void Stop()
        {
            LogEvent("Остановка ...");

            // удаление хвостов
            Active = false;
            try { Thread.Abort(); } catch { }
            try { Port.Close(); } catch { }

            // сигнализация о завершении
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

        SerialPort Port { get; set; }

        Thread Thread { get; set; }

        string[] Devices { get; set; }

        bool Active { get; set; } = false;

        void Monitoring()
        {
            while (Active)
            {
                var starttime = DateTime.Now;

                if (!Port.IsOpen)
                {
                    try { Port.Open(); }
                    catch (Exception e) 
                    {
                        LogEvent("COM порт не открыт: " + e.Message, LogType.ERROR);
                        try { Port.Close(); } catch { } 
                    }

                    Thread.Sleep(5000);
                }
                
                if (Port.IsOpen)
                {
                    foreach (var device in Devices)
                    {
                        if (Active)
                        {
                            try
                            {
                                Acquisition(device);
                            }
                            catch (Exception e)
                            {
                                LogEvent("COM порт не открыт: " + e.Message, LogType.ERROR);
                                try { Port.Close(); } catch { }
                            }

                            WriteValue(device + ".Time", DateTime.Now.ToString("HH:mm:ss"));
                            UpdateEvent();
                        }
                    }

                    WriteValue("Time", DateTime.Now.ToString("HH:mm:ss"));
                    UpdateEvent();

                    int timeout = Convert.ToInt32(Configuration.Timeout - (DateTime.Now - starttime).TotalMilliseconds);
                    if (timeout > 0) Thread.Sleep(timeout);
                }
            }
        }

        void Initialize()
        {
            foreach (var device in Devices)
            {
                WriteValue(device + ".WactiveIn", 0);
                WriteValue(device + ".WactiveOut", 0);
                WriteValue(device + ".WreactiveIn", 0);
                WriteValue(device + ".WreactiveOut", 0);
                WriteValue(device + ".Pin", 0);
                WriteValue(device + ".Pout", 0);
                WriteValue(device + ".Pa", 0);
                WriteValue(device + ".Pb", 0);
                WriteValue(device + ".Pc", 0);
                WriteValue(device + ".Qin", 0);
                WriteValue(device + ".Qout", 0);
                WriteValue(device + ".Qa", 0);
                WriteValue(device + ".Qb", 0);
                WriteValue(device + ".Qc", 0);
                WriteValue(device + ".Ua", 0);
                WriteValue(device + ".Ub", 0);
                WriteValue(device + ".Uc", 0);
                WriteValue(device + ".PhaseAB", 0);
                WriteValue(device + ".PhaseBC", 0);
                WriteValue(device + ".PhaseCA", 0);
                WriteValue(device + ".Ia", 0);
                WriteValue(device + ".Ib", 0);
                WriteValue(device + ".Ic", 0);
                WriteValue(device + ".F", 0);
                WriteValue(device + ".Time", DateTime.Now.ToString("HH:mm:ss"));
            }

            Write("Time", DateTime.Now.ToString("HH:mm:ss"));
            foreach (var field in Fields) field.Value.Quality = 0;
            UpdateEvent();
        }

        void Acquisition(string device)
        {
            byte[] id;
            string raw;
            LogEvent("Start " + device, LogType.DETAILED);

            id = Encoding.ASCII.GetBytes(device);
            Exchange(new byte[] { 0x2F, 0x3F, id[0], id[1], id[2], id[3], 0x21, 0x0D, 0x0A });
            Exchange(new byte[] { 0x06, 0x30, 0x35, 0x31, 0x0D, 0x0A });
            Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x4D, 0x4F, 0x44, 0x45, 0x4C, 0x28, 0x29, 0x03, 0x4A });

            WriteValue(device + ".WactiveIn", Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x45, 0x54, 0x30, 0x50, 0x45, 0x28, 0x29, 0x03, 0x37 }).ToValue());
            WriteValue(device + ".WactiveOut", Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x45, 0x54, 0x30, 0x50, 0x49, 0x28, 0x29, 0x03, 0x3B }).ToValue());
            WriteValue(device + ".WreactiveIn", Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x45, 0x54, 0x30, 0x51, 0x45, 0x28, 0x29, 0x03, 0x38 }).ToValue());
            WriteValue(device + ".WreactiveOut", Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x45, 0x54, 0x30, 0x51, 0x49, 0x28, 0x29, 0x03, 0x3C }).ToValue());

            raw = Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x50, 0x4F, 0x57, 0x45, 0x50, 0x28, 0x29, 0x03, 0x64 });
            WriteValue(device + ".Pin", raw.ToValue());
            WriteValue(device + ".Pout", raw.ToValue(2));

            raw = Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x50, 0x4F, 0x57, 0x50, 0x50, 0x28, 0x29, 0x03, 0x6F });
            WriteValue(device + ".Pa", raw.ToValue());
            WriteValue(device + ".Pb", raw.ToValue(2));
            WriteValue(device + ".Pc", raw.ToValue(3));

            raw = Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x50, 0x4F, 0x57, 0x45, 0x51, 0x28, 0x29, 0x03, 0x65 });
            WriteValue(device + ".Qin", raw.ToValue());
            WriteValue(device + ".Qout", raw.ToValue(2));

            raw = Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x50, 0x4F, 0x57, 0x50, 0x51, 0x28, 0x29, 0x03, 0x70 });
            WriteValue(device + ".Qa", raw.ToValue());
            WriteValue(device + ".Qb", raw.ToValue(2));
            WriteValue(device + ".Qc", raw.ToValue(3));

            raw = Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x56, 0x4F, 0x4C, 0x54, 0x41, 0x28, 0x29, 0x03, 0x5F });
            WriteValue(device + ".Ua", raw.ToValue());
            WriteValue(device + ".Ub", raw.ToValue(2));
            WriteValue(device + ".Uc", raw.ToValue(3));

            raw = Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x43, 0x4F, 0x52, 0x55, 0x55, 0x28, 0x29, 0x03, 0x67 });
            WriteValue(device + ".PhaseAB", raw.ToValue());
            WriteValue(device + ".PhaseBC", raw.ToValue(2));
            WriteValue(device + ".PhaseCA", raw.ToValue(3));

            raw = Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x43, 0x55, 0x52, 0x52, 0x45, 0x28, 0x29, 0x03, 0x5A });
            WriteValue(device + ".Ia", raw.ToValue());
            WriteValue(device + ".Ib", raw.ToValue(2));
            WriteValue(device + ".Ic", raw.ToValue(3));

            WriteValue(device + ".F", Exchange(new byte[] { 0x01, 0x52, 0x31, 0x02, 0x46, 0x52, 0x45, 0x51, 0x55, 0x28, 0x29, 0x03, 0x5C }).ToValue());

            Exchange(new byte[] { 0x01, 0x42, 0x30, 0x03, 0x75 });

            LogEvent("Complete " + device, LogType.DETAILED);
        }

        void WriteValue(string name, object value)
        {
            lock (Fields)
            {
                if (Fields.ContainsKey(name))
                {
                    if (Fields[name] != null)
					{
                        Fields[name].Value = value;
                        Fields[name].Quality = 192;
                    }
                    else
					{
                        Fields[name] = new DefField
                        {
                            Name = name,
                            Value = value,
                            Quality = 192,
                        };
					}
                }
                else
                {
                    Fields.Add(name, new DefField
                    {
                        Name = name,
                        Value = value,
                        Quality = 192,
                    });
                }
            }
        }

        string Exchange(byte[] tx)
        {
            Port.Write(tx, 0, tx.Length);
            LogEvent("TX: " + Helpers.BytesToString(tx), LogType.DETAILED);
            Thread.Sleep(Configuration.TX_Timeout);

            byte[] rx = new byte[Port.BytesToRead];
            Port.Read(rx, 0, rx.Length);
            LogEvent("RX: " + Helpers.BytesToString(rx), LogType.DETAILED);
            Thread.Sleep(Configuration.RX_Timeout);

            return Encoding.ASCII.GetString(rx);
        }

        bool Err(string text)
        {
            LogEvent(text, LogType.ERROR);
            return false;
        }

        void ComError(string errorName)
        {
            LogEvent("COM: " + errorName, LogType.DETAILED);
            Port.BaseStream.Flush();
            Port.DiscardInBuffer();
            Port.DiscardOutBuffer();
        }
    }
}