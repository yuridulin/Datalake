using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.METRAN_900
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

            Fields = new Dictionary<string, DefField>
            {
                { "Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss") }},
                { "DATE", new DefField { Value = "" }},
                { "K1", new DefField { Value = 0F }},
                { "K2", new DefField { Value = 0F }},
                { "K3", new DefField { Value = 0F }},
                { "K4", new DefField { Value = 0F }},
                { "K5", new DefField { Value = 0F } },
                { "K6", new DefField { Value = 0F }},
                { "K7", new DefField { Value = 0F }},
                { "K8", new DefField { Value = 0F }},
                { "K9", new DefField { Value = 0F }},
                { "K10", new DefField { Value = 0F }},
                { "K11", new DefField { Value = 0F }},
                { "K12", new DefField { Value = 0F }},
            };

            try { Port.Close(); } catch (Exception) { }
            Port = new SerialPort();
            Port.DataReceived += (s, e) => ReadAnswer();
            Port.ErrorReceived += (s, e) =>
            {
                LogEvent("Ошибка COM порта: " + e.EventType.ToString(), LogType.DETAILED);
                Port.BaseStream.Flush();
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
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

            try
            {
                Port.PortName = Configuration.PortName;
                Port.BaudRate = Configuration.BaudRate;
                Port.DataBits = Configuration.DataBits;
                Port.Parity = (Parity)Configuration.Parity;
                Port.StopBits = (StopBits)Configuration.StopBits;
                Port.ReadTimeout = Configuration.ReadTimeout;
                Port.WriteTimeout = Configuration.WriteTimeout;
            }
            catch (Exception e)
            {
                return Err("Ошибка при установке параметров COM порта: " + e.Message);
            }

            ExchangeActive = true;

            if (Thread != null)
            {
                Thread.Abort();
                Thread = null;
            }

            Thread = new Thread(SendRequestPackages) { IsBackground = true };
            Thread.Start();

            LogEvent("Мониторинг запущен");

            return true;
        }

        public void Stop()
        {
            LogEvent("Остановка ...");

            ExchangeActive = false;
            try { Port.Close(); } catch (Exception) { }

            LogEvent("Мониторинг остановлен");
        }

        public void Write(string fieldName, object value)
        {
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        DateTime StartDate { get; set; } = DateTime.Parse("01.01.2000 00:00:00");

        SerialPort Port { get; set; }
        
        Thread Thread { get; set; }

        DateTime ExchangeStartDate { get; set; }

        byte[] Package { get; set; } = new byte[] { 0x01, 0x55, 0x4D, 0xFF, 0xFF, 0x00, 0x00 };

        bool ExchangeActive { get; set; } = false;

        void SendRequestPackages()
        {
            while (ExchangeActive)
            {
                ExchangeStartDate = DateTime.Now;

                if (!Port.IsOpen)
                {
                    try
                    {
                        Port.Open();
                    }
                    catch (Exception e)
                    {
                        try { Port.Close(); } catch (Exception) { }
                        Err("Ошибка при открытии COM порта: " + e.Message + "\n" + e.StackTrace);
                        return;
                    }
                }

                if (Port.IsOpen)
                {
                    try
                    {
                        Port.Write(Package, 0, Package.Length);
                        LogEvent("Tx: " + Helpers.BytesToString(Package), LogType.DETAILED);
                    }
                    catch (Exception e)
                    {
                        Err("Ошибка при записи в COM порт: " + e.Message + "\n" + e.StackTrace);
                    }
                }

                lock (Fields)
                {
                    Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
                    Fields["Time"].Quality = 192;
                }

                int timeout = Convert.ToInt32(Configuration.Timeout - (DateTime.Now - ExchangeStartDate).TotalMilliseconds);
                if (timeout > 0) Thread.Sleep(timeout);
            }
        }

        void ReadAnswer()
        {
            try
            {
                lock (Fields)
                {
                    int _bytesToRead = Port.BytesToRead;
                    byte[] _bytes = new byte[_bytesToRead];
                    Port.Read(_bytes, 0, _bytesToRead);
                    if (_bytes.Length == 0) return;

                    LogEvent("Rx: " + Helpers.BytesToString(_bytes), LogType.DETAILED);

                    if (_bytesToRead != 33) return;

                    Fields["DATE"].Value = StartDate
                        .AddSeconds(BitConverter.ToInt32(new[] { _bytes[1], _bytes[2], _bytes[3], _bytes[4], }, 0))
                        .ToString("dd.MM.yyyy HH:mm:ss");
                    Fields["DATE"].Quality = 192;

                    for (int i = 0; i < 12; i++)
                    {
                        BitArray valueBits = new BitArray(new[] { _bytes[5 + i * 2], _bytes[6 + i * 2] });

                        var a = BitsToInt(new[] {
                            valueBits[0],
                            valueBits[1],
                            valueBits[2],
                            valueBits[3],
                            valueBits[4],
                        });

                        var b = BitsToInt(new[] {
                            valueBits[5],
                            valueBits[6],
                            valueBits[7],
                            valueBits[8],
                            valueBits[9],
                            valueBits[10],
                            valueBits[11],
                            valueBits[12],
                            valueBits[13],
                            valueBits[14],
                        });

                        float value = float.Parse(b + "," + (int)Math.Round(a / 0.31, 0));

                        Fields["K" + (i + 1)].Value = value;
                        Fields["K" + (i + 1)].Quality = 192;

                    }
                }

                UpdateEvent();
            }
            catch (Exception e)
            {
                Err("Ошибка при чтении из COM порта: " + e.Message + "\n" + e.StackTrace);
            }
        }

        double BitsToInt(bool[] bits)
        {
            double result = 0;

            try
            {
                for (int i = 0; i < bits.Length; i++)
                {
                    if (bits[i])
                    {
                        result += Math.Pow(2, i);
                    }
                }
            }
            catch (Exception e)
            {
                LogEvent("Ошибка при конвертации битов в число: " + e.Message, LogType.DETAILED);
            }

            return result;
        }

        bool Err(string text)
        {
            LogEvent(text, LogType.ERROR);
            return false;
        }
    }
}