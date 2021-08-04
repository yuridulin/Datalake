using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using iNOPC.Library;
using Newtonsoft.Json;

namespace iNOPC.Drivers.MARK_602
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

            Prepare();
            try { Port.Close(); } catch (Exception) { }

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
                NetworkAddress = Configuration.NetworkAddress;
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
            WinLogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        SerialPort Port { get; set; }

        Thread Thread { get; set; }

        DateTime ExchangeStartDate { get; set; }

        bool ExchangeActive { get; set; } = false;

        byte NetworkAddress { get; set; }

        const int msBetweenFields = 50;

        List<Field> DefaultFields { get; set; } = new List<Field>
        {
            new Field
            {
                Name = "T_A",
                Channel = 1,
                RequestCode = 3,
                OperationCode = 131,
                Ratio = 0.01F
            },
            new Field
            {
                Name = "Salt_A",
                Channel = 1,
                RequestCode = 4,
                OperationCode = 132,
                Ratio = 1,
            },
            new Field
            {
                Name = "X_A",
                Channel = 1,
                RequestCode = 5,
                OperationCode = 133,
                Ratio = 1,
            },
            new Field
            {
                Name = "X25_A",
                Channel = 1,
                RequestCode = 6,
                OperationCode = 134,
                Ratio = 1,
            },
            new Field
            {
                Name = "K_Termo_A",
                Channel = 1,
                RequestCode = 7,
                OperationCode = 135,
                Ratio = 1,
            },
            new Field
            {
                Name = "Const_Sensor_A",
                Channel = 1,
                RequestCode = 8,
                OperationCode = 136,
                Ratio = 1,
            },
            new Field
            {
                Name = "TermoR_A",
                Channel = 1,
                RequestCode = 9,
                OperationCode = 137,
                Ratio = 1,
            },
            new Field
            {
                Name = "RefTermoR_A",
                Channel = 1,
                RequestCode = 10,
                OperationCode = 138,
                Ratio = 1,
            },
            new Field
            {
                Name = "DiapA",
                Channel = 1,
                RequestCode = 11,
                OperationCode = 139,
                Ratio = 1,
            },
            new Field
            {
                Name = "MAX_A",
                Channel = 1,
                RequestCode = 12,
                OperationCode = 140,
                Ratio = 1,
            },
            new Field
            {
                Name = "MIN_A",
                Channel = 1,
                RequestCode = 13,
                OperationCode = 141,
                Ratio = 1,
            },

            new Field
            {
                Name = "T_B",
                Channel = 2,
                RequestCode = 3,
                OperationCode = 131,
                Ratio = 0.01F,
            },
            new Field
            {
                Name = "Salt_B",
                Channel = 2,
                RequestCode = 4,
                OperationCode = 132,
                Ratio = 1,
            },
            new Field
            {
                Name = "X_B",
                Channel = 2,
                RequestCode = 5,
                OperationCode = 133,
                Ratio = 1,
            },
            new Field
            {
                Name = "X25_B",
                Channel = 2,
                RequestCode = 6,
                OperationCode = 134,
                Ratio = 1,
            },
            new Field
            {
                Name = "K_Termo_B",
                Channel = 2,
                RequestCode = 7,
                OperationCode = 135,
                Ratio = 1,
            },
            new Field
            {
                Name = "Const_Sensor_B",
                Channel = 2,
                RequestCode = 8,
                OperationCode = 136,
                Ratio = 1,
            },
            new Field
            {
                Name = "TermoR_B",
                Channel = 2,
                RequestCode = 9,
                OperationCode = 137,
                Ratio = 1,
            },
            new Field
            {
                Name = "RefTermoR_B",
                Channel = 2,
                RequestCode = 10,
                OperationCode = 138,
                Ratio = 1,
            },
            new Field
            {
                Name = "DiapB",
                Channel = 2,
                RequestCode = 11,
                OperationCode = 139,
                Ratio = 1,
            },
            new Field
            {
                Name = "MAX_B",
                Channel = 2,
                RequestCode = 12,
                OperationCode = 140,
                Ratio = 1,
            },
            new Field
            {
                Name = "MIN_B",
                Channel = 2,
                RequestCode = 13,
                OperationCode = 141,
                Ratio = 1,
            },
        };

        void Prepare()
        {
            Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss") });
            foreach (var field in DefaultFields) Fields.Add(field.Name, new DefField { Value = 0F });

            Port = new SerialPort();
            Port.DataReceived += (s, e) => ReadAnswer();
            Port.ErrorReceived += (s, e) =>
            {
                LogEvent("Ошибка COM порта: " + e.EventType.ToString(), LogType.DETAILED);
                Port.BaseStream.Flush();
                Port.DiscardInBuffer();
                Port.DiscardOutBuffer();
            };
        }

        void SendRequestPackages()
        {
            while (ExchangeActive)
            {
                ExchangeStartDate = DateTime.Now;

                if (!Port.IsOpen)
                {
                    try { Port.Open(); }
                    catch (Exception)
                    {
                        try { Port.Close(); } catch (Exception) { }
                        Err("Ошибка при открытии COM порта");
                    }
                }

                if (Port.IsOpen)
                {
                    foreach (var field in DefaultFields)
                    {
                        if (!ExchangeActive) break;

                        SendRequestPackage(field);

                        Thread.Sleep(msBetweenFields);
                    }

                    lock (Fields)
                    {
                        Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
                    }

                    UpdateEvent();
                }

                int timeout = Convert.ToInt32(Configuration.Timeout - (DateTime.Now - ExchangeStartDate).TotalMilliseconds);
                if (timeout > 0) Thread.Sleep(timeout);
            }

            try { Port.Close(); } catch (Exception) { }
        }

        void SendRequestPackage(Field field)
        {
            try
            {
                var package = BuildRequestPackage(field);
                LogEvent("Tx: " + Helpers.BytesToString(package), LogType.DETAILED);

                Port.Write(package, 0, package.Length);
            }
            catch (Exception e)
            {
                LogEvent("Tx: ошибка [" + e.Message + "]", LogType.DETAILED);
            }
        }

        byte[] BuildRequestPackage(Field field)
        {
            byte[] _bytes = new byte[9];

            _bytes[0] = 255;
            _bytes[1] = NetworkAddress;
            _bytes[2] = field.Channel;
            _bytes[3] = field.RequestCode;
            _bytes[4] = 0;
            _bytes[5] = 0;
            _bytes[6] = 0;
            _bytes[7] = 0;
            _bytes[8] = CRC(_bytes, 8);

            return _bytes;
        }

        void ReadAnswer()
        {
            int bytesToRead = Port.BytesToRead;
            if (bytesToRead >= 9)
            {
                byte[] package = new byte[9];
                Port.Read(package, 0, 9);

                LogEvent("Rx: " + Helpers.BytesToString(package), LogType.DETAILED);
                ParseAnswerPackage(package);
            }
            else if (bytesToRead > 0)
            {
                byte[] package = new byte[bytesToRead];
                Port.Read(package, 0, bytesToRead);

                LogEvent("Rx: получен неполный пакет [" + Helpers.BytesToString(package) + "]", LogType.DETAILED);
            }
            else
            {
                LogEvent("Rx: ложное срабатывание", LogType.DETAILED);
            }
        }

        void ParseAnswerPackage(byte[] package)
        {
            byte crc = CRC(package, 8);
            byte preambula = 255;
            byte networkAddress = NetworkAddress;

            if (package[0] != preambula)
            {
                LogEvent("Преамбула не равна 255: " + package[0], LogType.DETAILED);
            }
            else if (package[1] != networkAddress)
            {
                LogEvent("Сетевой адрес ответа не совпадает с сетевым адресом посылки: " + package[1] + " != " + networkAddress, LogType.DETAILED);
            }
            else if (package[8] != crc)
            {
                LogEvent("Контрольная сумма не валидна: полученная " + package[8] + ", расчетная " + crc, LogType.DETAILED);
            }
            else
            {
                byte channel = package[2];
                byte code = package[3];
                float value = 0;
                try { value = BitConverter.ToSingle(package, 4); } catch (Exception) { }

                var field = DefaultFields.FirstOrDefault(x => x.Channel == channel && x.OperationCode == code);

                lock (Fields)
                {
                    Fields[field.Name].Value = Math.Round(value * field.Ratio, 5);
                }
            }
        }

        byte CRC(byte[] _bytes, int _length = 0)
        {
            if (_length == 0) _length = _bytes.Length;
            byte _crc = 1;
            for (int _i = 0; _i < _length; _i++) _crc += (byte)(_bytes[_i] ^ 0xFF);
            return _crc;
        }

        bool Err(string text)
        {
            LogEvent(text, LogType.ERROR);
            return false;
        }
    }
}