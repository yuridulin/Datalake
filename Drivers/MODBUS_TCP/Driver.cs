using iNOPC.Drivers.MODBUS_TCP.Models;
using iNOPC.Library;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace iNOPC.Drivers.MODBUS_TCP
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

            // чтение конфигурации
            try
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
            }
            catch (Exception e)
            {
                return Err("Конфигурация не прочитана: " + e.Message + "\n" + e.StackTrace);
            }

            if (Configuration.Fields.Count == 0)
            {
                return Err("Список опрашиваемых полей пуст");
            }
            else if (!Fields.ContainsKey("Time"))
            {
                Fields.Add("Time", new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 });
            }

            UpdateEvent();

            try 
			{
				Packages.Clear();
				CreatePackages();
			}
            catch (Exception e)
            {
                return Err("Ошибка при обработке полей: " + e.Message + "\n" + e.StackTrace);
            }

            try
            {
                if (Thread != null)
                {
                    Thread.Abort();
                    Thread = null;
                }

                Active = true;
                ErrCount = 0;

                Thread = new Thread(() =>
                {
                    while (Active)
                    {
                        Monitoring();
                    }
                });
                Thread.Start();

                LogEvent("Мониторинг запущен");

                return true;
            }
            catch (Exception e)
            {
                return Err("Неизвестная ошибка: " + e.Message + "\n" + e.StackTrace);
            }
        }

        public void Stop()
        {
            LogEvent("Остановка ...");

            Active = false;

            if (Thread != null)
            {
                Thread.Abort();
                Thread = null;
            }

            if (Client != null && Client.Connected)
            {
                Client.Close();
                Client = null;
            }

            LogEvent("Мониторинг остановлен");
        }

        public void Write(string fieldName, object value)
        {
            LogEvent("Событие записи в поле [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]", LogType.DETAILED);
        }


        // Реализация получения данных

        Configuration Configuration { get; set; } = null;

        TcpClient Client { get; set; }

        NetworkStream Stream { get; set; }

        Thread Thread { get; set; }

        List<Package> Packages { get; set; } = new List<Package>();

        DateTime RequestStart { get; set; }

        bool Active { get; set; }

        int ErrCount { get; set; } = 0;

        void CreatePackages()
        {
            // подготовка полей к опросу
            // формирование пакетов, по которым будут запрашиваться данные

            var checkedFields = Fields.Keys;

            foreach (Field field in Configuration.Fields) field.Checked = false;

            ushort i = 0;

            if (Configuration.Multicast)
            {
                Configuration.Fields = Configuration.Fields.OrderBy(x => x.Address).ToList();

                while (Configuration.Fields.Count(x => !x.Checked) > 0)
                {
                    Field[] fields = Configuration.Fields.Where(x => !x.Checked).ToArray();

                    Package package = new Package
                    {
						Transaction = i,
						StartAddress = fields[0].Address,
                        OldByteFirst = Configuration.OldByteFirst,
                        OldRegisterFirst = Configuration.OldRegisterFirst,
                    };

                    byte length = GetRegistersCount(fields[0].Type);

                    package.Parts.Add(new PackagePart
                    {
                        FieldName = fields[0].Name,
                        Length = length,
                        Type = fields[0].Type,
                        Scale = fields[0].Scale,
                    });

                    fields[0].Checked = true;

                    for (int k = 1; k < Math.Min(fields.Length, 61); k++)
                    {
                        if (fields[k].Address == fields[k - 1].Address + length)
                        {
                            byte fieldKlength = GetRegistersCount(fields[k].Type);

                            package.Parts.Add(new PackagePart
                            {
                                FieldName = fields[k].Name,
                                Length = fieldKlength,
                                Type = fields[k].Type,
                                Scale = fields[k].Scale,
                            });

                            length = fieldKlength;

                            fields[k].Checked = true;
                        }
                        else { break; }
                    }

                    Packages.Add(package);

                    i++;
                }
            }
            else
            {
                foreach (Field field in Configuration.Fields.Where(x => !x.Checked))
                {
                    Packages.Add(new Package
                    {
                        Transaction = i,
                        StartAddress = field.Address,
                        OldByteFirst = Configuration.OldByteFirst,
                        OldRegisterFirst = Configuration.OldRegisterFirst,
                        Parts = new List<PackagePart>
                        {
                            new PackagePart
                            {
                                FieldName = field.Name,
                                Length = GetRegistersCount(field.Type),
                                Type = field.Type,
                                Scale = field.Scale,
                            }
                        },
                    });

                    field.Checked = true;
                    i++;
                }
            }

            foreach (Package package in Packages) package.Construct();

            Fields.Clear();
            Fields["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 };
            foreach (Field field in Configuration.Fields) Fields.Add(field.Name, new DefField { Value = 0F, Quality = 0 });
        }

        byte GetRegistersCount(string type)
        {
            switch (type)
            {
                case nameof(Byte):
                case nameof(Int16):
                case nameof(UInt16):
                    return 1;

                case nameof(Int32):
                case nameof(UInt32):
                case nameof(Single):
                case nameof(DateTime):
                default:
                    return 2;

                case nameof(Int64):
                case nameof(UInt64):
                case nameof(Double):
                case "Int.Int":
                case "TM2Date":
                    return 4;
            }
        }

        bool Connect()
        {
            try
            {
                if (Client?.Connected != true)
                {
                    try
                    {
                        Client?.Close();
                        Client = null;

                        Stream = null;
                    }
                    catch { }

                    Client = new TcpClient();
                    Client.Connect(Configuration.Ip, Configuration.Port);

                    if (Client?.Connected == true)
                    {
                        if (Stream == null)
                        {
                            Stream = Client.GetStream();
                        }
                    }
                }

                if (Client?.Connected == true && Stream != null) return true;

                return false;
            }
            catch (Exception e)
            {
                LogEvent("Ошибка подключения: " + e.Message + "\n" + e.StackTrace, LogType.ERROR);
                return false;
            }
        }

        void Monitoring()
        {
            RequestStart = DateTime.Now;
            bool isConnected = Connect();
            bool hasErr = false;

            if (isConnected)
            {
                lock (Fields)
                {
                    foreach (Package package in Packages)
                    {
                        if (!Active) break;
                        try
                        {
                            byte[] command = package.Trancieve;
                            Stream.Write(command, 0, command.Length);
                            LogEvent("Tx: " + Helpers.BytesToString(command), LogType.DETAILED);

                            DateTime d = DateTime.Now;
                            while ((DateTime.Now - d).TotalSeconds < 2 && !Stream.DataAvailable)
                            {
                                Thread.Sleep(10);
                            }

                            if (Stream.DataAvailable)
                            {
                                int receiveLength = package.ReceiveLength;
                                byte[] answer = new byte[receiveLength];
                                Stream.Read(answer, 0, receiveLength);
                                LogEvent("Rx: " + Helpers.BytesToString(answer), LogType.DETAILED);

                                if (command[0] == answer[0] && command[1] == answer[1])
                                {
                                    package.Receive(answer);
                                    foreach (PackagePart part in package.Parts)
                                    {
                                        if (Fields.ContainsKey(part.FieldName))
                                        {
                                            Fields[part.FieldName].Value = part.Value;
                                            Fields[part.FieldName].Quality = 192;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                LogEvent("Данные не вернулись по таймауту", LogType.DETAILED);
                                hasErr = true;
                            }
                        }
                        catch (Exception e)
                        {
                            LogEvent("Ошибка при опросе значений: " + e.Message + "\n" + e.StackTrace, LogType.DETAILED);
                            hasErr = true;
                        }
                    }
                    Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
                }
            }

            if (hasErr) ErrCount++; else ErrCount = 0;
            if (ErrCount > 5)
            {
                Reconnect();
                return;
            }

            if (!hasErr) UpdateEvent();
            double ms = (DateTime.Now - RequestStart).TotalMilliseconds;

            int timeout = Convert.ToInt32(Configuration.CyclicTimeout - ms);
            if (timeout > 0) Thread.Sleep(timeout);
        }

        void Reconnect()
        {
            try
            {
                Client.Close();
                Client = null;
                Stream = null;
            }
            catch { }
        }

        bool Err(string text)
        {
            LogEvent(text, LogType.ERROR);
            return false;
        }
    }
}