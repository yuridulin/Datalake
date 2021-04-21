using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace RVP_18
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

            // подготовка объекта COM порта к использованию
            try { Port.Close(); } catch (Exception) { }
            Port = new SerialPort();
            Port.DataReceived += (s, e) => Receive();
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

            UpdateEvent();

            // установка начальных значений
            try
            {
                Port.PortName = Configuration.PortName;
                Port.BaudRate = Configuration.BaudRate;
                Port.DataBits = Configuration.DataBits;
                Port.Parity = (Parity)Configuration.Parity;
                Port.StopBits = (StopBits)Configuration.StopBits;
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

            Thread = new Thread(Trancieve);
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

		void Receive()
		{
            var bytes = new List<byte>();
            while (Port.BytesToRead > 0)
			{
                bytes.Add((byte)Port.ReadByte());
			}

            // разбор пакета
            LogEvent("Tx: " + Helpers.BytesToString(bytes.ToArray()));
		}

        void Trancieve()
		{

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

        public static ushort CRC(byte[] bytes, int len)
		{
			ushort crc = 0xFFFF;

			for (int pos = 0; pos < len; pos++)
			{
				crc ^= bytes[pos];

				for (int i = 8; i > 0; i--)
				{
					if ((crc & 0x8000) > 0)
					{
						crc <<= 1;
						crc ^= 0x1021;
					}
					else
					{
						crc <<= 1;
					}
				}
			}

			return crc;
		}
	}
}