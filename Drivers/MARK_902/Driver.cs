using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace iNOPC.Drivers.MARK_902
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



        // реализация получения данных

        Configuration Configuration { get; set; }

        SerialPort Port { get; set; }

        Thread Thread { get; set; }

        DateTime ExchangeStartDate { get; set; }

        bool ExchangeActive { get; set; } = false;

        byte NetworkAddress { get; set; }

        List<Field> DefaultFields { get; set; } = new List<Field>
        {
            new Field
            {
                Name = "EMF_A",
                Channel = 1,
                RequestCode = 3,
                OperationCode = 131,
                Ratio = 1,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "T_A",
                Channel = 1,
                RequestCode = 4,
                OperationCode = 132,
                Ratio = 0.1F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "pH_A",
                Channel = 1,
                RequestCode = 5,
                OperationCode = 133,
                Ratio = 0.01F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "pH25_A",
                Channel = 1,
                RequestCode = 6,
                OperationCode = 134,
                Ratio = 0.01F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "S_A",
                Channel = 1,
                RequestCode = 7,
                OperationCode = 135,
                Ratio = 1,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "Ei_A",
                Channel = 1,
                RequestCode = 8,
                OperationCode = 136,
                OnlySecondByte = true,
                Ratio = 1,
            },
            new Field
            {
                Name = "miniDIAP_A",
                Channel = 1,
                RequestCode = 9,
                OperationCode = 137,
                Ratio = 0.1F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "widthDIAP_A",
                Channel = 1,
                RequestCode = 10,
                OperationCode = 138,
                Ratio = 0.1F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "MAX_A",
                Channel = 1,
                RequestCode = 11,
                OperationCode = 139,
                Ratio = 0.1F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "MIN_A",
                Channel = 1,
                RequestCode = 12,
                OperationCode = 140,
                Ratio = 0.1F,
                OnlySecondByte = false,
            },

            new Field
            {
                Name = "EMF_B",
                Channel = 2,
                RequestCode = 3,
                OperationCode = 131,
                Ratio = 1,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "T_B",
                Channel = 2,
                RequestCode = 4,
                OperationCode = 132,
                Ratio = 0.1F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "pH_B",
                Channel = 2,
                RequestCode = 5,
                OperationCode = 133,
                Ratio = 0.01F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "pH25_B",
                Channel = 2,
                RequestCode = 6,
                OperationCode = 134,
                Ratio = 0.01F
            },
            new Field
            {
                Name = "S_B",
                Channel = 2,
                RequestCode = 7,
                OperationCode = 135,
                Ratio = 1,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "Ei_B",
                Channel = 2,
                RequestCode = 8,
                OperationCode = 136,
                OnlySecondByte = true,
                Ratio = 1,
            },
            new Field
            {
                Name = "miniDIAP_B",
                Channel = 2,
                RequestCode = 9,
                OperationCode = 137,
                Ratio = 0.1F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "widthDIAP_B",
                Channel = 2,
                RequestCode = 10,
                OperationCode = 138,
                Ratio = 0.1F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "MAX_B",
                Channel = 2,
                RequestCode = 11,
                OperationCode = 139,
                Ratio = 0.1F,
                OnlySecondByte = false,
            },
            new Field
            {
                Name = "MIN_B",
                Channel = 2,
                RequestCode = 12,
                OperationCode = 140,
                Ratio = 0.1F,
                OnlySecondByte = false,
            },
        };

        const int msBetweenFields = 200;

        void Prepare()
        {
            Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss") });
            foreach (var def in DefaultFields)
            {
                Fields.Add(def.Name, new DefField { Value = 0F });
            }

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
                    catch (Exception e)
                    {
                        try { Port.Close(); } catch (Exception) { }
                        Err("Ошибка при открытии COM порта: " + e.Message);
                    }
                }

                if (Port.IsOpen)
                {
                    foreach (Field field in DefaultFields)
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
            byte[] _bytes = new byte[7];

            _bytes[0] = 255;
            _bytes[1] = NetworkAddress;
            _bytes[2] = field.Channel;
            _bytes[3] = field.RequestCode;
            _bytes[4] = 0;
            _bytes[5] = 0;
            _bytes[6] = CRC(_bytes, 5, 0);

            return _bytes;
        }

        void ReadAnswer()
        {
            int bytesToRead = Port.BytesToRead;
            if (bytesToRead >= 7)
            {
                byte[] package = new byte[7];
                Port.Read(package, 0, 7);

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
            byte crc = CRC(package, 6, 1);
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
            else if (package[6] != crc)
            {
                LogEvent("Контрольная сумма не валидна: полученная " + package[6] + ", расчетная " + crc, LogType.DETAILED);
            }
            else
            {
                byte channel = package[2];
                byte code = package[3];

                var field = DefaultFields.FirstOrDefault(x => x.Channel == channel && x.OperationCode == code);

                float value = 0;

                if (field.OnlySecondByte)
                {
                    value = ValueFromBytes(new byte[] { package[5], 0, });
                }
                else
                {
                    value = ValueFromBytes(new byte[] { package[5], package[4], });
                }

                lock (Fields)
                {
                    Fields[field.Name].Value = Math.Round(value * field.Ratio, 5);
                }
            }
        }

        int ToNumeral(bool[] _bits)
        {
            BitArray _binary = new BitArray(_bits);
            try
            {
                int[] _result = new int[1];
                _binary.CopyTo(_result, 0);
                return _result[0];
            }
            catch (Exception)
            {
                string bits = "";
                for (int _i = 0; _i < _binary.Count; _i++) bits += _binary[_i] ? "1" : "0";
                LogEvent("Ошибка конвертации массива битов в число 1: " + bits, LogType.DETAILED);
                return 0;
            }
        }

        int ValueFromBytes(byte[] bytes)
        {
            int[] numericValues = new int[4] { 0, 0, 0, 0 };
            bool lessThanZero = false;

            BitArray _binary = new BitArray(bytes);

            try
            {
                numericValues[0] = ToNumeral(new[] { _binary[0], _binary[1], _binary[2], _binary[3], });
                numericValues[1] = ToNumeral(new[] { _binary[4], _binary[5], _binary[6], _binary[7], });
                numericValues[2] = ToNumeral(new[] { _binary[8], _binary[9], _binary[10], _binary[11], });
                numericValues[3] = ToNumeral(new[] { _binary[12], _binary[13], _binary[14], false, });

                int result =
                    (1 * numericValues[0])
                    + (10 * numericValues[1])
                    + (100 * numericValues[2])
                    + (1000 * numericValues[3]);

                lessThanZero = _binary[15];
                if (lessThanZero) result = 0 - result;

                return result;
            }
            catch (Exception)
            {
                string bits = "";
                for (int _i = 0; _i < _binary.Count; _i++) bits += _binary[_i] ? "1" : "0";
                LogEvent("Ошибка конвертации массива битов в число 2: " + bits, LogType.DETAILED);

                return 0;
            }
        }

        byte CRC(byte[] _bytes, int _length = 0, byte _start = 1)
        {
            if (_length == 0) _length = _bytes.Length;

            // Начальное значение разное из-за ошибки на приборе
            // он ожидает CRC как инверсную сумму, а возвращает как инверсную сумму + 1
            byte _crc = _start;
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