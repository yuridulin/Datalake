using System;
using System.Collections.Generic;

namespace iNOPC.Drivers.MODBUS_TCP.Models
{
    public class Package
    {
        public ushort Transaction { get; set; } = 0;

        public ushort StartAddress { get; set; } = 0;

        public List<PackagePart> Parts { get; set; } = new List<PackagePart>();

        public byte[] Trancieve { get; set; } = new byte[0];

        public int ReceiveLength { get; set; } = 0;

        public byte CommandCode { get; set; } = 0;

        public bool OldRegisterFirst { get; set; } = false;

        public bool OldByteFirst { get; set; } = false;


        public void Construct()
        {
            // Определение кода команды на чтение по характеру запрашиваемых данных, если код команды не задан в конфиге
            if (CommandCode == 0)
            {
                if (StartAddress >= 0 && StartAddress < 10000) CommandCode = 0x01;
                else if (StartAddress >= 10000 && StartAddress < 20000) CommandCode = 0x02;
                else if (StartAddress >= 30000 && StartAddress < 40000) CommandCode = 0x04;
                else if (StartAddress >= 40000 && StartAddress < 50000) CommandCode = 0x03;
            }

            // Определение смещения относительно начального адреса
            byte _length = 0;
            foreach (PackagePart part in Parts) _length += part.Length;

            // Определение количества байт данных, получаемых в ответе
            // начальное значение - (SlaveId = 1) + (функц. код = 1) + (кол-во значимых байтов = 1) + (CRC = 2)
            // умножение на 2 - потому что в одном регистре Modbus таблицы хранится 2 байта
            ReceiveLength = 9;
            foreach (PackagePart part in Parts) ReceiveLength += 2 * part.Length;

            // Получение номера транзакции
            byte[] _transaction = BitConverter.GetBytes(Transaction);

            // Составление команды на получение данных
            byte[] _address = BitConverter.GetBytes(StartAddress);

            Trancieve = new byte[]
            {
                // номер транзакции
                _transaction[1],
                _transaction[0],
                // код протокола (Modbus RTU и Modbus TCP - всегда 00 00)
                0,
                0,
                // длина оставшейся части команды (адрес = 1, функц. код = 1, адрес = 2, кол-во регистров = 2)
                0,
                6,
                // сетевой адрес устройства
                1,
                // команда на чтение Input Registers (полей с численными значениями)
                4,
                // стартовый адрес, начиная с которого будут забираться данные
                _address[1],
                _address[0],
                // кол-во байтов, которые необходимо забрать, зависит от типа забираемого значения и их кол-ва
                0,
                _length,
            };
        }

        public void Receive(byte[] bytes)
        {
            int position = 9;
            foreach (PackagePart part in Parts)
            {
                object value = part.Value;

                try
                {
                    // умножение на 2 делается потому, что в одном адресе Modbus таблицы хранится 2 байта
                    int bytesCount = 2 * part.Length;
                    byte[] valueBytes = new byte[bytesCount];

                    for (int i = 0; i < bytesCount; i++)
                    {
                        valueBytes[i] = bytes[position + i];
                    }

                    byte[] toValue;

                    switch (part.Type)
                    {
                        case nameof(Byte):
                            value = OldByteFirst ? valueBytes[1] : valueBytes[0];
                            break;

                        case nameof(DateTime):
                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst ? new[]
                                {
                                valueBytes[3],
                                valueBytes[2],
                                valueBytes[1],
                                valueBytes[0],
                            } :
                                new[]
                                {

                                valueBytes[2],
                                valueBytes[3],
                                valueBytes[0],
                                valueBytes[1],
                                };
                            }
                            else
                            {
                                toValue = OldByteFirst ? new[]
                                {
                                valueBytes[1],
                                valueBytes[0],
                                valueBytes[3],
                                valueBytes[2],
                            } :
                                new[]
                                {
                                valueBytes[0],
                                valueBytes[1],
                                valueBytes[2],
                                valueBytes[3],
                                };
                            }

                            value = (new DateTime(1970, 1, 1, 3, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromSeconds(BitConverter.ToInt32(toValue, 0))).ToString("HH:mm:ss");
                            break;

                        case nameof(Int16):
                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[1],
                                    valueBytes[0],
                                    }
                                    : new[]
                                    {
                                    valueBytes[0],
                                    valueBytes[1],
                                    };
                            }
                            else
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[1],
                                    valueBytes[0],
                                    }
                                    : new[]
                                    {
                                    valueBytes[0],
                                    valueBytes[1],
                                    };
                            }

                            var _int16 = BitConverter.ToInt16(toValue, 0);
                            value = part.Scale != 0 ? (_int16 / part.Scale) : _int16;
                            break;

                        case nameof(UInt16):
                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[1],
                                    valueBytes[0],
                                    }
                                    : new[]
                                    {
                                    valueBytes[0],
                                    valueBytes[1],
                                    };
                            }
                            else
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[1],
                                    valueBytes[0],
                                    }
                                    : new[]
                                    {
                                    valueBytes[0],
                                    valueBytes[1],
                                    };
                            }

                            var _uint16 = BitConverter.ToUInt16(toValue, 0);
                            value = part.Scale != 0 ? (_uint16 / part.Scale) : _uint16;
                            break;

                        case nameof(Int32):
                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[3],
                                    valueBytes[2],
                                    valueBytes[1],
                                    valueBytes[0],
                                    }
                                    : new[]
                                    {
                                    valueBytes[2],
                                    valueBytes[3],
                                    valueBytes[0],
                                    valueBytes[1],
                                    };
                            }
                            else
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[1],
                                    valueBytes[0],
                                    valueBytes[3],
                                    valueBytes[2],
                                    }
                                    : new[]
                                    {
                                    valueBytes[0],
                                    valueBytes[1],
                                    valueBytes[2],
                                    valueBytes[3],
                                    };
                            }

                            var _int32 = BitConverter.ToInt32(toValue, 0);
                            value = part.Scale != 0 ? (_int32 / part.Scale) : _int32;
                            break;

                        case nameof(UInt32):
                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[3],
                                    valueBytes[2],
                                    valueBytes[1],
                                    valueBytes[0],
                                    }
                                    : new[]
                                    {
                                    valueBytes[2],
                                    valueBytes[3],
                                    valueBytes[0],
                                    valueBytes[1],
                                    };
                            }
                            else
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[1],
                                    valueBytes[0],
                                    valueBytes[3],
                                    valueBytes[2],
                                    }
                                    : new[]
                                    {
                                    valueBytes[0],
                                    valueBytes[1],
                                    valueBytes[2],
                                    valueBytes[3],
                                    };
                            }

                            var _uint32 = BitConverter.ToUInt32(toValue, 0);
                            value = part.Scale != 0 ? (_uint32 / part.Scale) : _uint32;
                            break;

                        case nameof(Int64):
                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst ? new[]
                                {
                                valueBytes[7],
                                valueBytes[6],
                                valueBytes[5],
                                valueBytes[4],
                                valueBytes[3],
                                valueBytes[2],
                                valueBytes[1],
                                valueBytes[0],
                            } :
                                new[]
                                {
                                valueBytes[6],
                                valueBytes[7],
                                valueBytes[4],
                                valueBytes[5],
                                valueBytes[2],
                                valueBytes[3],
                                valueBytes[0],
                                valueBytes[1],
                                };
                            }
                            else
                            {
                                toValue = OldByteFirst ? new[]
                                {
                                valueBytes[1],
                                valueBytes[0],
                                valueBytes[3],
                                valueBytes[2],
                                valueBytes[5],
                                valueBytes[4],
                                valueBytes[7],
                                valueBytes[6],
                            } :
                                new[]
                                {
                                valueBytes[0],
                                valueBytes[1],
                                valueBytes[2],
                                valueBytes[3],
                                valueBytes[4],
                                valueBytes[5],
                                valueBytes[6],
                                valueBytes[7],
                                };
                            }

                            var _int64 = BitConverter.ToInt64(toValue, 0);
                            value = part.Scale != 0 ? (_int64 / part.Scale) : _int64;
                            break;

                        case nameof(UInt64):
                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst ? new[]
                                {
                                valueBytes[7],
                                valueBytes[6],
                                valueBytes[5],
                                valueBytes[4],
                                valueBytes[3],
                                valueBytes[2],
                                valueBytes[1],
                                valueBytes[0],
                            } :
                                new[]
                                {
                                valueBytes[6],
                                valueBytes[7],
                                valueBytes[4],
                                valueBytes[5],
                                valueBytes[2],
                                valueBytes[3],
                                valueBytes[0],
                                valueBytes[1],
                                };
                            }
                            else
                            {
                                toValue = OldByteFirst ? new[]
                                {
                                valueBytes[1],
                                valueBytes[0],
                                valueBytes[3],
                                valueBytes[2],
                                valueBytes[5],
                                valueBytes[4],
                                valueBytes[7],
                                valueBytes[6],
                            } :
                                new[]
                                {
                                valueBytes[0],
                                valueBytes[1],
                                valueBytes[2],
                                valueBytes[3],
                                valueBytes[4],
                                valueBytes[5],
                                valueBytes[6],
                                valueBytes[7],
                                };
                            }

                            var _uint64 = BitConverter.ToUInt64(toValue, 0);
                            value = part.Scale != 0 ? (_uint64 / part.Scale) : _uint64;
                            break;

                        case nameof(Double):
                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst ? new[]
                                {
                                valueBytes[7],
                                valueBytes[6],
                                valueBytes[5],
                                valueBytes[4],
                                valueBytes[3],
                                valueBytes[2],
                                valueBytes[1],
                                valueBytes[0],
                            } :
                                new[]
                                {
                                valueBytes[6],
                                valueBytes[7],
                                valueBytes[4],
                                valueBytes[5],
                                valueBytes[2],
                                valueBytes[3],
                                valueBytes[0],
                                valueBytes[1],
                                };
                            }
                            else
                            {
                                toValue = OldByteFirst ? new[]
                                {
                                valueBytes[1],
                                valueBytes[0],
                                valueBytes[3],
                                valueBytes[2],
                                valueBytes[5],
                                valueBytes[4],
                                valueBytes[7],
                                valueBytes[6],
                            } :
                                new[]
                                {
                                valueBytes[0],
                                valueBytes[1],
                                valueBytes[2],
                                valueBytes[3],
                                valueBytes[4],
                                valueBytes[5],
                                valueBytes[6],
                                valueBytes[7],
                                };
                            }

                            var _double = BitConverter.ToDouble(toValue, 0);
                            value = part.Scale != 0 ? (_double / part.Scale) : _double;
                            break;

                        case nameof(Single):
                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[3],
                                    valueBytes[2],
                                    valueBytes[1],
                                    valueBytes[0],
                                    }
                                    : new[]
                                    {
                                    valueBytes[2],
                                    valueBytes[3],
                                    valueBytes[0],
                                    valueBytes[1],
                                    };
                            }
                            else
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[1],
                                    valueBytes[0],
                                    valueBytes[3],
                                    valueBytes[2],
                                    }
                                    : new[]
                                    {
                                    valueBytes[0],
                                    valueBytes[1],
                                    valueBytes[2],
                                    valueBytes[3],
                                    };
                            }

                            var _single = BitConverter.ToSingle(toValue, 0);
                            value = part.Scale != 0 ? (_single / part.Scale) : _single;
                            break;

                        case "Int.Int":
                            int first = 0, second = 0;

                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[3],
                                    valueBytes[2],
                                    valueBytes[1],
                                    valueBytes[0],
                                    }
                                    : new[]
                                    {
                                    valueBytes[2],
                                    valueBytes[3],
                                    valueBytes[0],
                                    valueBytes[1],
                                    };
                            }
                            else
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[1],
                                    valueBytes[0],
                                    valueBytes[3],
                                    valueBytes[2],
                                    }
                                    : new[]
                                    {
                                    valueBytes[0],
                                    valueBytes[1],
                                    valueBytes[2],
                                    valueBytes[3],
                                    };
                            }

                            second = BitConverter.ToInt32(toValue, 0);

                            if (OldRegisterFirst)
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[7],
                                    valueBytes[6],
                                    valueBytes[5],
                                    valueBytes[4],
                                    }
                                    : new[]
                                    {
                                    valueBytes[6],
                                    valueBytes[7],
                                    valueBytes[4],
                                    valueBytes[5],
                                    };
                            }
                            else
                            {
                                toValue = OldByteFirst
                                    ? new[]
                                    {
                                    valueBytes[5],
                                    valueBytes[4],
                                    valueBytes[7],
                                    valueBytes[6],
                                    }
                                    : new[]
                                    {
                                    valueBytes[4],
                                    valueBytes[5],
                                    valueBytes[6],
                                    valueBytes[7],
                                    };
                            }

                            first = BitConverter.ToInt32(toValue, 0);

                            var _intint = Convert.ToDouble(first.ToString() + "," + second.ToString());
                            value = part.Scale != 0 ? (_intint / part.Scale) : _intint;
                            break;

                        case "TM2Date":
                            value = new DateTime(valueBytes[5], valueBytes[4], valueBytes[3], valueBytes[2], valueBytes[1], valueBytes[0]).ToString("HH:mm:ss");
                            break;
                    }

                    position += bytesCount;
                }
                catch
                {
                    value = part.Value;
                }

                part.Value = value;
            }
        }
    }
}