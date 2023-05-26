using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Drivers.Elemer_IRT5501M1
{
	public class Driver : IDriver
    {
        public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

        public Dictionary<string, DefField> Fields { get; set; }

		public event LogEvent LogEvent;

		public event UpdateEvent UpdateEvent;

		public bool Start(string jsonConfiguration)
		{
            LogEvent("Запуск ...");

            try { Port?.Close(); } catch { }
            Port = new SerialPort();
            Port.DataReceived += (s, e) => IsReceived = true;
            Port.ErrorReceived += (s, e) => LogEvent("COM error: " + e.EventType.ToString(), LogType.ERROR);

            // чтение конфигурации
            try
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfiguration);
            }
            catch (Exception e)
            {
                return Err("Конфигурация не прочитана: " + e.Message);
            }

            // подготовка пакетов для запроса
            Fields = new Dictionary<string, DefField>
            {
                { "Time", new DefField { Quality = 192, Value = DateTime.Now.ToString("HH:mm:ss") } },
                { "Channel1", new DefField { Quality = 0, Value = 0F } },
            };
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

            IsActive = true;

            ExchangeTimer = new Timer(Configuration.Timeout);
            ExchangeTimer.Elapsed += (s, e) => Exchange();
            ExchangeTimer.Start();

            LogEvent("Мониторинг запущен");

            Task.Run(Exchange);

            return true;
        }

		public void Stop()
		{
            LogEvent("Остановка ...");

            IsActive = false;
            try { ExchangeTimer?.Stop(); } catch { }
            try { ExchangeTimer = null; } catch { }
            try { Port?.Close(); } catch { }
            try { Port = null; } catch { }

            lock (Fields)
			{
                Fields["Channel1"].Quality = 0;
            }
            UpdateEvent();

            LogEvent("Мониторинг остановлен");
        }

		public void Write(string fieldName, object value)
		{
			LogEvent("Запись не поддерживается", LogType.ERROR);
		}

        // realization

        Configuration Configuration { get; set; }

        SerialPort Port { get; set; }

        Timer ExchangeTimer { get; set; }

        bool IsReceived { get; set; }

        bool IsActive { get; set; }

        void Exchange()
		{
            if (!IsActive) return;

            try
			{
                if (!Port.IsOpen)
				{
                    try
                    {
                        Port.Open();
                    }
                    catch (Exception e)
                    {
                        Err("Reconnect: " + e.Message + "\n" + e.StackTrace);
                        try { Port.Close(); } catch { }
                        return;
                    }
                }

                if (Port.IsOpen)
				{
                    IsReceived = false;

                    Port.Write(new byte[] { 0xFF, 0x3A, 0x31, 0x3B, 0x31, 0x3B, 0x30, 0x3B, 0x37, 0x36, 0x32, 0x37, 0x0D }, 0, 13);

                    DateTime reqTime = DateTime.Now;

                    while (IsActive && ((DateTime.Now - reqTime).TotalMilliseconds <= Math.Min(2000, 0.75 * Configuration.Timeout) || !IsReceived))
					{
                        Task.Delay(100).Wait();
					}

                    if (IsReceived)
                    {
                        int toRead = Port.BytesToRead;

                        byte[] tx = new byte[toRead];
                        Port.Read(tx, 0, toRead);

                        try
                        {
                            string raw = Encoding.ASCII.GetString(tx);
                            LogEvent("Ответ: " + raw, LogType.DETAILED);

                            raw = raw.Substring(raw.IndexOf(';') + 1);
                            raw = raw.Substring(0, raw.IndexOf(';'));
                            LogEvent("Разобранный ответ: " + raw, LogType.DETAILED);

                            float value = Convert.ToSingle(raw.Replace('.', ','));
                            LogEvent("Значение: " + value, LogType.DETAILED);

                            lock (Fields)
							{
                                Fields["Channel1"].Value = value;
                                Fields["Channel1"].Quality = 192;
                            }
                        }
                        catch (Exception e)
						{
                            Err("Ошибка при разборе ответа: " + e.Message);
						}
                    }
                    else
					{
                        LogEvent("Ответ не вернулся по таймауту", LogType.WARNING);
                    }
				}
            }
            catch (Exception e)
			{
                if (!IsActive) return;
                Err("Ошибка при обмене: " + e.Message + "\n" + e.StackTrace);
			}

            lock (Fields)
			{
                Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
                Fields["Time"].Quality = 192;
            }

            UpdateEvent();
        }

        bool Err(string message)
		{
            LogEvent(message, LogType.ERROR);
            return false;
		}
    }
}
