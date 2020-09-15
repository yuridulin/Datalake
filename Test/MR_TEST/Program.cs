using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace MR_TEST
{
    public class Driver
    {
        public List<Field> Fields { get; set; }

        public SerialPort Port { get; set; }

        public bool Received { get; set; }

        public bool Active { get; set; }

        public Thread Thread { get; set; }

        public void Start()
        {
            Fields = new List<Field>
            {
                new Field { Name = "Дата-время", Address = 0x0200, Type = FieldType.DATE },

                new Field { Name = "Неисправность", Address = 0x1802 },

                new Field { Name = "Реле аварии", Address = 0x1822 },
                new Field { Name = "Реле сигнализации", Address = 0x1823 },
                new Field { Name = "Индикатор 1", Address = 0x1824 },
                new Field { Name = "Индикатор 2", Address = 0x1825 },

                new Field { Name = "Программируемый индикатор 3", Address = 0x1828 },
                new Field { Name = "Программируемый индикатор 4", Address = 0x1829 },
                new Field { Name = "Программируемый индикатор 5", Address = 0x182A },
                new Field { Name = "Программируемый индикатор 6", Address = 0x182B },
                new Field { Name = "Программируемый индикатор 7", Address = 0x182C },
                new Field { Name = "Программируемый индикатор 8", Address = 0x182D },
                new Field { Name = "Программируемый индикатор 9", Address = 0x182E },
                new Field { Name = "Программируемый индикатор 10", Address = 0x182F },

                new Field { Name = "Состояние выходного реле 3", Address = 0x1832 },
                new Field { Name = "Состояние выходного реле 4", Address = 0x1833 },
                new Field { Name = "Состояние выходного реле 6", Address = 0x1835 },
                new Field { Name = "Состояние выходного реле 7", Address = 0x1836 },
                new Field { Name = "Состояние выходного реле 12", Address = 0x183B },
                new Field { Name = "Состояние выходного реле 13", Address = 0x183C },
                new Field { Name = "Состояние выходного реле 14", Address = 0x183D },
                new Field { Name = "Состояние выходного реле 15", Address = 0x183E },
                new Field { Name = "Состояние выходного реле 16", Address = 0x183F },
                new Field { Name = "Неисправность устройства (аппаратная)", Address = 0x1840 },
                new Field { Name = "Неисправность (ошибка задачи логики)", Address = 0x1841 },
                new Field { Name = "Неисправность устройства (данных)", Address = 0x1842 },
                new Field { Name = "Неисправность измерения (напряжения)", Address = 0x1846 },
                new Field { Name = "Неисправность измерения (частоты)", Address = 0x1847 },

                new Field { Name = "U[ABC] < 5V", Address = 0x1873 },

                new Field { Name = "Сигнализация (запись в журнал аварий)", Address = 0x1882 },
                new Field { Name = "Авария (аварийное отключение)", Address = 0x1883 },

                new Field { Name = "Дискретный сигнал Д1", Address = 0x1888 },
                new Field { Name = "Дискретный сигнал Д2", Address = 0x1889 },
                new Field { Name = "Дискретный сигнал Д3", Address = 0x188A },
                new Field { Name = "Дискретный сигнал Д4", Address = 0x188B },
                new Field { Name = "Дискретный сигнал Д5", Address = 0x188C },
                new Field { Name = "Дискретный сигнал Д6", Address = 0x188D },
                new Field { Name = "Дискретный сигнал Д8", Address = 0x188F },

                new Field { Name = "Логический сигнал Л5", Address = 0x1894 },
                new Field { Name = "Логический сигнал Л6", Address = 0x1895 },

                new Field { Name = "Вых. логический сигнал ВЛС1", Address = 0x1898 },
                new Field { Name = "Вых. логический сигнал ВЛС8", Address = 0x189F },

                new Field { Name = "U[min] U> ИО", Address = 0x18A8 },
                new Field { Name = "U[min] U> СРАБ", Address = 0x18A9 },

                new Field { Name = "F[min] F< ИО", Address = 0x18C8 },
                new Field { Name = "F[min] F< СРАБ", Address = 0x18C9 },
                new Field { Name = "F[min] F<< ИО", Address = 0x18CA },
                new Field { Name = "F[min] F<< СРАБ", Address = 0x18CB },

                new Field { Name = "ВЗ-1 СРАБ", Address = 0x18D0 },
                new Field { Name = "ВЗ-2 СРАБ", Address = 0x18D1 },
                new Field { Name = "ВЗ-3 СРАБ", Address = 0x18D2 },
                new Field { Name = "ВЗ-4 СРАБ", Address = 0x18D3 },

                new Field { Name = "Импульсный сигнал возврата U0>", Address = 0x18F8 },

                new Field { Name = "Напряжение U[n]", Address = 0x1900, Type = FieldType.WORD },
                new Field { Name = "Напряжение U[a]", Address = 0x1902, Type = FieldType.WORD },
                new Field { Name = "Напряжение U[b]", Address = 0x1904, Type = FieldType.WORD },
                new Field { Name = "Напряжение U[c]", Address = 0x1906, Type = FieldType.WORD },
                new Field { Name = "Напряжение прямой последовательности U1", Address = 0x1911, Type = FieldType.WORD },
                new Field { Name = "Напряжение обратной последовательности U2", Address = 0x1913, Type = FieldType.WORD },
                new Field { Name = "Частота F", Address = 0x1915, Type = FieldType.WORD },

            };

            Port = new SerialPort
            {
                PortName = "COM2",
                BaudRate = 115200,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
            };
            Port.DataReceived += (s, e) => Received = true;
            Port.Open();

            Thread = new Thread(() =>
            {
                Active = true;
                while (Active)
                {
                    foreach (var field in Fields)
                    {
                        Exchange(field);
                        Thread.Sleep(100);
                    }
                    Thread.Sleep(10000);
                }
            });
            Thread.Start();
        }

        public void Stop()
        {
            Active = false;

            Thread.Abort();
            Port.Close();
        }

        public void Exchange(Field field)
        {
            if (Received) return;

            byte[] _address = BitConverter.GetBytes(field.Address);
            byte[] _length = BitConverter.GetBytes(field.GetBytesLength());
            byte[] _bytes = { 1, field.GetReadCommand(), _address[1], _address[0], _length[1], _length[0], };
            byte[] _crc = BitConverter.GetBytes(CRC(_bytes, 6));

            Port.Write(new byte[] { _bytes[0], _bytes[1], _bytes[2], _bytes[3], _bytes[4], _bytes[5], _crc[0], _crc[1], }, 0, 8);

            while (!Received && Active) Thread.Sleep(10);
            Received = false;

            byte[] _output = new byte[Port.BytesToRead];
            Port.Read(_output, 0, _output.Length);

            List<byte> raws = new List<byte>();
            for (int i = 3; i < _output.Length - 2; i++)
            {
                raws.Add(_output[i]);
            }
            _output = raws.ToArray();

            switch (field.Type)
            {
                case FieldType.DATE:
                    field.Value = new DateTime(
                        (2000 + BitConverter.ToInt16(new byte[] { _output[1], _output[0] }, 0)),
                        BitConverter.ToInt16(new byte[] { _output[3], _output[2] }, 0),
                        BitConverter.ToInt16(new byte[] { _output[5], _output[4] }, 0),
                        BitConverter.ToInt16(new byte[] { _output[7], _output[6] }, 0),
                        BitConverter.ToInt16(new byte[] { _output[9], _output[8] }, 0),
                        BitConverter.ToInt16(new byte[] { _output[11], _output[10] }, 0)
                    );
                    break;

                case FieldType.WORD:
                    field.Value = BitConverter.ToInt16(new byte[] { _output[1], _output[0] }, 0);
                    break;

                case FieldType.BIT:
                    field.Value = _output[0];
                    break;
            }

            Console.WriteLine(field.Name + " = " + field.Value);
        }

        public static ushort CRC(byte[] bytes, int len)
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

    }

    class Program
    {
        static void Main()
        {
            var driver = new Driver();

            driver.Start();
            Console.ReadLine();
            driver.Stop();
        }
    }
}