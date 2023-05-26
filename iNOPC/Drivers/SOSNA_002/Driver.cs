using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.SOSNA_002
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

            try { Port.Close(); } catch (Exception) { }

            CreateFields();

            rawOutput = "";
            device = 1;
            tryCount = 0;
            previousDevice = 0;

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

            Timer.Interval = Configuration.Timeout;
            Timer.Start();

            Task.Run(SendRequest);

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
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        SerialPort Port { get; set; }

        Timer Timer { get; set; }

        byte device;

        string rawOutput;

        int tryCount;

        int previousDevice;

        void CreateFields()
        {
            var date = DateTime.Now.ToString("HH:mm:ss");

            lock (Fields)
            {
                Fields.Clear();
                Fields.Add("Time", new DefField { Value = date });

                Fields.Add("s1_dt", new DefField { Value = date });
                Fields.Add("s1_k1", new DefField { Value = 0F });
                Fields.Add("s1_k2", new DefField { Value = 0F });
                Fields.Add("s1_k3", new DefField { Value = 0F });

                Fields.Add("s2_dt", new DefField { Value = date });
                Fields.Add("s2_k1", new DefField { Value = 0F });
                Fields.Add("s2_k2", new DefField { Value = 0F });
                Fields.Add("s2_k3", new DefField { Value = 0F });
                Fields.Add("s2_k4", new DefField { Value = 0F });

                Fields.Add("s3_dt", new DefField { Value = date });
                Fields.Add("s3_k1", new DefField { Value = 0F });
                Fields.Add("s3_k2", new DefField { Value = 0F });
                Fields.Add("s3_k3", new DefField { Value = 0F });
                Fields.Add("s3_k4", new DefField { Value = 0F });
            }

            Port = new SerialPort();
            Port.DataReceived += (s, e) => GetAnswer();
            Port.ErrorReceived += (s, e) =>
            {
                LogEvent("Ошибка COM порта: " + e.EventType.ToString(), LogType.DETAILED);
                Port.BaseStream.Flush();
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
            };

            Timer = new Timer();
            Timer.Elapsed += (s, e) => SendRequest();
        }

        void SendRequest()
        {
            try
            {
                if (!Port.IsOpen)
                {
                    try
                    {
                        Port.Open();
                        tryCount = 0;
                    }
                    catch (Exception)
                    {
                        Err("errorНе удалось открыть COM порт");
                        return;
                    }
                }

                Port.BaseStream.Flush();
                Port.Write(new byte[] { device, 1, 0, 0, 1 }, 0, 5);
                LogEvent("Tx: " + Helpers.BytesToString(new byte[] { device, 1, 0, 0, 1 }) + " (канал " + device + ")", LogType.DETAILED);

                if (device == 3)
                {
                    device = 1;
                }
                else
                {
                    device++;
                }

            }
            catch (Exception e)
            {
                Err("Ошибка при отправке запроса [" + e.Message + "]");
            }
        }

        void GetAnswer()
        {
            try
            {
                rawOutput += Port.ReadExisting();
                LogEvent("Rx: " + rawOutput, LogType.DETAILED);

                int start = rawOutput.IndexOf('?');
                int end = rawOutput.LastIndexOf('?');

                if (start > -1 && end > -1 && start != end)
                {
                    if ((end - start) > 2)
                    {
                        string answer = rawOutput.Substring(start + 1, end - 1);

                        byte[] bytes = Encoding.UTF8.GetBytes(answer).ToList().Where(b => b != 0).ToArray();
                        answer = Encoding.UTF8.GetString(bytes);
                        ParseAnswer(answer);
                    }

                    rawOutput = rawOutput.Substring(end);
                }
            }
            catch (Exception e)
            {
                Err("Ошибка при получении ответа [" + e.Message + "]");
            }
        }

        void ParseAnswer(string answer)
        {
            try
            {
                string[] values = answer.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                int deviceNumber = int.TryParse(values[0], out int i) ? i : 0;

                if (deviceNumber != previousDevice)
                {
                    previousDevice = deviceNumber;

                    lock (Fields)
                    {
                        if (deviceNumber == 1 && values.Length == 4)
                        {
                            Fields["s1_dt"].Value = DateTime.Now.ToString("HH:mm:ss");
                            Fields["s1_dt"].Quality = 192;
                            Fields["s1_k1"].Value = Number(values[1]);
                            Fields["s1_k1"].Quality = 192;
                            Fields["s1_k2"].Value = Number(values[2]);
                            Fields["s1_k2"].Quality = 192;
                            Fields["s1_k3"].Value = Number(values[3]);
                            Fields["s1_k3"].Quality = 192;
                            LogEvent("Получены значения для канала " + deviceNumber, LogType.DETAILED);
                            tryCount = 0;
                        }
                        else if (deviceNumber == 2 && values.Length == 5)
                        {
                            Fields["s2_dt"].Value = DateTime.Now.ToString("HH:mm:ss");
                            Fields["s2_dt"].Quality = 192;
                            Fields["s2_k1"].Value = Number(values[1]);
                            Fields["s2_k1"].Quality = 192;
                            Fields["s2_k2"].Value = Number(values[2]);
                            Fields["s2_k2"].Quality = 192;
                            Fields["s2_k3"].Value = Number(values[3]);
                            Fields["s2_k3"].Quality = 192;
                            Fields["s2_k4"].Value = Number(values[4]);
                            Fields["s2_k4"].Quality = 192;
                            LogEvent("Получены значения для канала " + deviceNumber, LogType.DETAILED);
                            tryCount = 0;
                        }
                        else if (deviceNumber == 3 && values.Length == 5)
                        {
                            Fields["s3_dt"].Value = DateTime.Now.ToString("HH:mm:ss");
                            Fields["s3_dt"].Quality = 192;
                            Fields["s3_k1"].Value = Number(values[1]);
                            Fields["s3_k1"].Quality = 192;
                            Fields["s3_k2"].Value = Number(values[2]);
                            Fields["s3_k2"].Quality = 192;
                            Fields["s3_k3"].Value = Number(values[3]);
                            Fields["s3_k3"].Quality = 192;
                            Fields["s3_k4"].Value = Number(values[4]);
                            Fields["s3_k4"].Quality = 192;
                            LogEvent("Получены значения для канала " + deviceNumber, LogType.DETAILED);
                            tryCount = 0;
                        }
                        else
                        {
                            tryCount++;
                            LogEvent("Ответ без значений для канала " + deviceNumber, LogType.DETAILED);
                        }

                        if (tryCount > 10)
                        {
                            Err("Данные не приходят");
                            try { Port.Close(); } catch (Exception) { }
                        }

                        Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
                        Fields["Time"].Quality = 192;
                    }

                    UpdateEvent();
                }
                else
                {
                    if (deviceNumber == 1 && values.Length == 4)
                    {
                        tryCount = 0;
                    }
                    else if (deviceNumber == 2 && values.Length == 5)
                    {
                        tryCount = 0;
                    }
                    else if (deviceNumber == 3 && values.Length == 5)
                    {
                        tryCount = 0;
                    }
                    else
                    {
                        tryCount++;
                        LogEvent("Попытка #" + tryCount, LogType.DETAILED);
                    }
                }
            }
            catch (Exception e)
            {
                Err("Ошибка при получении значений запроса [" + e.Message + "]");
            }
        }

        float Number(string s)
        {
            try
            {
                return float.TryParse(s.Replace(".", ","), out float f) ? f : 0;
            }
            catch (Exception e)
            {
                Err("Ошибка при конвертировании числа [" + e.Message + "]");
                return 0;
            }
        }

        bool Err(string text)
        {
            LogEvent(text, LogType.ERROR);
            return false;
        }
    }
}