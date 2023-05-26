using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Timers;

namespace COM_Echo
{
	public class Driver : IDriver
	{
        public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

		public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

		public event LogEvent LogEvent;
		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfiguration)
		{
            LogEvent("Запуск ...");

            try { Port?.Close(); } catch { }

            CreateFields();

            try
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfiguration);
            }
            catch (Exception e)
            {
                return Err("Конфигурация не прочитана: " + e.Message + "\n" + e.StackTrace);
            }

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
                return Err("Ошибка при установке параметров COM порта: " + e.Message);
            }

            Timer.Interval = Configuration.Interval * 1000;
            Timer.Start();

            Task.Run(Exchange);

            LogEvent("Мониторинг запущен");

            return true;
        }

		public void Stop()
		{
            LogEvent("Остановка ...");

            Timer.Stop();
            try { Port.Close(); } catch (Exception) { }

            LogEvent("Мониторинг остановлен");
        }

		public void Write(string fieldName, object value)
		{
			throw new NotImplementedException();
		}


		Configuration Configuration { get; set; }

        SerialPort Port { get; set; }

        Timer Timer { get; set; }

        void CreateFields()
        {
            lock (Fields)
            {
                Fields?.Clear();
                Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 0 });
                Fields.Add("Echo", new DefField { Value = false, Quality = 0 });
            }

            UpdateEvent();

            Port = new SerialPort();
            Port.ErrorReceived += (s, e) =>
            {
                LogEvent("Ошибка COM порта: " + e.EventType.ToString(), LogType.DETAILED);
                Port.BaseStream.Flush();
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
            };

            Timer = new Timer();
            Timer.Elapsed += (s, e) => Exchange();
        }

        void Exchange()
		{
            try
            {
                if (!Port.IsOpen)
                {
                    try
                    {
                        Port.Open();
                    }
                    catch (Exception)
                    {
                        Err("errorНе удалось открыть COM порт");
                        return;
                    }
                }

                string date = DateTime.Now.ToString("HH:mm:ss");

                Port.BaseStream.Flush();
                Port.Write(date);
                LogEvent("TX: " + date, LogType.DETAILED);

                Task.Delay(Configuration.EchoDelay).Wait();

                string echo = Port.ReadExisting();
                LogEvent("RX: " + echo + " [" + (date == echo ? "true" : "false") + "]", LogType.DETAILED);

                Fields["Echo"].Value = date == echo;
                Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");

                Fields["Echo"].Quality = 192;
                Fields["Time"].Quality = 192;
            }
            catch (Exception e)
            {
                Err("Ошибка при отправке запроса [" + e.Message + "]");
                Fields["Echo"].Quality = 0;
                Fields["Time"].Quality = 0;
            }
			finally
			{
                UpdateEvent();
			}
        }

        bool Err(string message)
		{
            LogEvent(message, LogType.ERROR);
            return false;
		}
	}
}
