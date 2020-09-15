using iNOPC.Drivers.MR_NETWORK.Models;
using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace iNOPC.Drivers.MR_NETWORK
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

            try { Port.Close(); } catch (Exception) { }
            CreateFields();

            try
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
            }
            catch (Exception e)
            {
                return Err("Конфигурация не прочитана: " + e.Message + "\n" + e.StackTrace);
            }

            try
            {
                Port.PortName = Configuration.PortName;
                Port.BaudRate = Configuration.BaudRate;
                Port.Parity = (Parity)Configuration.Parity;
                Port.DataBits = Configuration.DataBits;
                Port.StopBits = (StopBits)Configuration.StopBits;

                foreach (var field in Configuration.Fields)
                {
                    field.ParseHex();
                }
            }
            catch (Exception e)
            {
                return Err("Ошибка при инициализации параметров: " + e.Message + "\n" + e.StackTrace);
            }

            if (Thread != null)
            {
                try
                {
                    Thread.Abort();
                    Thread = null;
                }
                catch (Exception) { }
            }

            Thread = new Thread(Exchange) { IsBackground = true };
            Thread.Start();

            LogEvent("Мониторинг запущен");

            return true;
        }

        public void Stop()
        {
            LogEvent("Остановка ...");

            Active = false;
            Thread.Abort();
            try { Port.Close(); } catch (Exception) { }

            LogEvent("Мониторинг остановлен");
        }

        public void Write(string fieldName, object value)
        {
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
            WinLogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
        }


        // реализация получения данных

        Configuration Configuration { get; set; }

        SerialPort Port { get; set; }

        Thread Thread { get; set; }

        bool Active { get; set; }

        bool Received { get; set; }

        void CreateFields()
        {
            Fields = new Dictionary<string, object>
            {
                { "Time", DateTime.Now.ToString("HH:mm:ss") }
            };

            Port = new SerialPort();
            Port.DataReceived += (s, e) => Received = true;
            Port.ErrorReceived += (s, e) =>
            {
                LogEvent("Ошибка COM порта: " + e.EventType.ToString(), LogType.DETAILED);
                Port.BaseStream.Flush();
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
            };
        }

        private void Exchange()
        {
            Active = true;

            int errorsCounter = 0;

            while (Active)
            {
                var date = DateTime.Now;

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
                    foreach (var field in Configuration.Fields)
                    {
                        try
                        {
                            GetValue(field);
                        }
                        catch (Exception e)
                        {
                            errorsCounter++;
                            LogEvent("Ошибка при опросе значения \"" + field.Name + "\": " + e.Message + "\n" + e.StackTrace, LogType.DETAILED);
                        }
                        Thread.Sleep(Configuration.FieldsInterval);
                    }
                }

                lock (Fields)
                {
                    foreach (var field in Configuration.Fields)
                    {
                        if (Fields.ContainsKey(field.Name))
                        {
                            Fields[field.Name] = field.Value;
                        }
                        else
                        {
                            Fields.Add(field.Name, field.Value);
                        }
                    }

                    Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
                }

                UpdateEvent();

                int timeout = Convert.ToInt32(Configuration.Timeout - (DateTime.Now - date).TotalMilliseconds);
                if (timeout > 0) Thread.Sleep(timeout);

                if (errorsCounter > 20)
                {
                    Port.Close();
                    errorsCounter = 0;
                    Thread.Sleep(1000);
                }
            }
        }

        private void GetValue(Field field)
        {
            if (Received) return;

            byte[] _address = BitConverter.GetBytes(field.Hex);
            byte[] _length = BitConverter.GetBytes(field.GetBytesLength());
            byte[] _tx = { Configuration.DeviceNumber, field.GetReadCommand(), _address[1], _address[0], _length[1], _length[0], };
            byte[] _crc = BitConverter.GetBytes(CRC(_tx, 6));
            byte[] tx = { _tx[0], _tx[1], _tx[2], _tx[3], _tx[4], _tx[5], _crc[0], _crc[1], };

            LogEvent("Tx: " + Helpers.BytesToString(tx), LogType.DETAILED);
            Port.Write(tx, 0, tx.Length);

            var date = DateTime.Now;
            while (!Received && Active && (DateTime.Now - date).TotalMilliseconds < Configuration.ReceiveTimeout)
            {
                Thread.Sleep(1);
            }

            Received = false;

            byte[] rx = new byte[Port.BytesToRead];
            Port.Read(rx, 0, rx.Length);
            LogEvent("Rx: " + Helpers.BytesToString(rx));

            List<byte> raws = new List<byte>();
            for (int i = 3; i < rx.Length - 2; i++)
            {
                raws.Add(rx[i]);
            }
            rx = raws.ToArray();

            switch (field.Type)
            {
                case FieldType.DATE:
                    field.Value = new DateTime(
                        (2000 + BitConverter.ToInt16(new byte[] { rx[1], rx[0] }, 0)),
                        BitConverter.ToInt16(new byte[] { rx[3], rx[2] }, 0),
                        BitConverter.ToInt16(new byte[] { rx[5], rx[4] }, 0),
                        BitConverter.ToInt16(new byte[] { rx[7], rx[6] }, 0),
                        BitConverter.ToInt16(new byte[] { rx[9], rx[8] }, 0),
                        BitConverter.ToInt16(new byte[] { rx[11], rx[10] }, 0)
                    );
                    break;

                case FieldType.WORD:
                    field.Value = BitConverter.ToInt16(new byte[] { rx[1], rx[0] }, 0);
                    break;

                case FieldType.BIT:
                    field.Value = rx[0];
                    break;
            }
        }

        private static ushort CRC(byte[] bytes, int len)
        {
            ushort crc = 0xFFFF;
            for (int pos = 0; pos < len; pos++)
            {
                crc ^= bytes[pos];
                for (int i = 8; i != 0; i--)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else crc >>= 1;
                }
            }
            //return (ushort)((crc >> 8) | (crc << 8));
            return crc;
        }

        bool Err(string text)
        {
            LogEvent(text, LogType.ERROR);
            return false;
        }
    }
}