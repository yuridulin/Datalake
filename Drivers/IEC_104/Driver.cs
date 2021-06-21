using iNOPC.Library;
using lib60870;
using lib60870.CS101;
using lib60870.CS104;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace iNOPC.Drivers.IEC_104
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

            // чтение конфигурации
            try
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
            }
            catch (Exception e)
            {
                return Err("Конфигурация не прочитана: " + e.Message);
            }

            // Создание полей, ранее заданных в конфиге
            Fields = new Dictionary<string, DefField>();
            try
            {
                foreach (var field in Configuration.NamedFields)
                {
                    switch (field.Type)
                    {
                        case "Bool":
                            Fields.Add(field.Name, new DefField { Value = false });
                            break;

                        case "Float":
                            Fields.Add(field.Name, new DefField { Value = 0F });
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                return Err("Ошибка при создании заданных полей: " + e.Message);
            }

            // Настройка таймеров
            ConnectionTimer = new Timer(Configuration.ReconnectTimeout * 1000);
            ConnectionTimer.Elapsed += (s, e) => CheckConnection();

            InterrogationTimer = new Timer(Configuration.InterrogationTimeout * 1000);
            InterrogationTimer.Elapsed += (s, e) => SendInterrogation();

            ClockTimer = new Timer(Configuration.SyncClockTimeout * 1000);
            ClockTimer.Elapsed += (s, e) => SyncClock();

            //try
            //{
            //    ResetConnection();
            //}
            //catch (Exception e)
            //{
            //    return Err("Подключение не создано: " + e.Message);
            //}

            //lock (Fields)
            //{
            //    Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
            //    Fields["Connection"] = false;
            //}
            //UpdateEvent();

            // запуск опроса
            IsActive = true;
            ConnectionPreviousStatus = false;
            ConnectionTimer.Start();
            InterrogationTimer.Start();
            ClockTimer.Start();

            LogEvent("Мониторинг запущен");
            Task.Run(CheckConnection);
            return true;
        }

        public void Stop()
        {
            LogEvent("Мониторинг останавливается...");

            IsActive = false;

            try { ConnectionTimer.Stop(); } catch { }
            try { InterrogationTimer.Stop(); } catch { }
            try { ClockTimer.Stop(); } catch { }
            try { Conn.Close(); } catch { }

            ClearFields();
            LogEvent("Мониторинг остановлен");
        }

        public void Write(string fieldName, object value)
        {
            // Поиск адреса поля по имени в списке именованных полей
            int address = 0;
            foreach (var field in Configuration.NamedFields)
            {
                if (field.Name == fieldName)
                {
                    address = field.Address;
                    break;
                }
            }

            // Проверка валидности переданного адреса (оно же имя поля, если имя не было найдено ранее)
            if (address == 0)
            {
                if (!int.TryParse(fieldName, out address))
                {
                    Err("Ошибка при записи: переданное имя поля [" + fieldName + "] не может быть преобразовано в адрес");
                    return;
                }
            }

            // Определение типа записываемого поля
            LogEvent("Запрос на запись [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
            WinLogEvent("Запрос на запись [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");

            // Отправление управляющей команды к серверу
            Task.Run(() =>
            {
                // Отслеживание количества попыток записи
                byte counter = 0;

                // Условие на прекращение записи после 5 фэйлов
                while (counter < 5)
                {
                    if (Conn.IsRunning)
                    {
                        try
                        {
                            bool command = true;

                            if (value.GetType().Equals(typeof(bool)))
                            {
                                command = (bool)value;
                                Conn.SendControlCommand(CauseOfTransmission.ACTIVATION, 1, new SingleCommand(address, command, false, 0));
                            }
                            else if (value.GetType().Equals(typeof(int)))
                            {
                                command = (int)value == 1;
                            }
                            else if (value.GetType().Equals(typeof(float)))
                            {
                                command = (float)value == 1;
                            }
                            else if (value.GetType().Equals(typeof(double)))
                            {
                                command = (double)value == 1;
                            }

                            LogEvent("Успешная запись [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
                            WinLogEvent("Успешная запись [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
                            counter = 6;
                        }
                        catch (Exception e)
                        {
                            Err("Ошибка при записи: " + e.Message);
                            counter++;
                        }
                    }
                    else
                    {
                        Task.Delay(100).Wait();
                        counter++;
                    }
                }

                if (counter != 6)
                {
                    LogEvent("Запись не удалась [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
                    WinLogEvent("Запись не удалась [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
                }
            });
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        Timer ConnectionTimer { get; set; }

        Timer InterrogationTimer { get; set; }

        Timer ClockTimer { get; set; }

        Connection Conn { get; set; }

        bool ConnectionPreviousStatus { get; set; }

        bool IsActive { get; set; } = false;

        bool ResetConnection()
		{
            try
            {
                // Очистка предыдущего подключения
                try { Conn?.Close(); } catch { }
                try { Conn = null; } catch { }

                var apci = new APCIParameters
                {
                    T0 = Configuration.ConnectionTimeoutT0, // задаётся в конфиге в секундах
                    T1 = Configuration.TimeoutT1,
                    T2 = Configuration.TimeoutT2,
                    T3 = Configuration.TimeoutT3,
                    K = 12,
                    W = 8,
                };

                Conn = new Connection(Configuration.Host, Configuration.Port, apci, new ApplicationLayerParameters { })
                {
                    DebugOutput = Configuration.DebugOutput,
                    Autostart = true,
                    ReceiveTimeout = Configuration.ReceiveTimeout,
                };

                Conn.SetASDUReceivedHandler(AsduReceivedHandler, null);
                Conn.SetReceivedRawMessageHandler(RawMessageHandler, "RX");
                Conn.SetSentRawMessageHandler(RawMessageHandler, "TX");
                Conn.Connect();

				lock (Fields)
				{
					Fields["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 };
					Fields["Connection"] = new DefField { Value = true, Quality = 192 };
                    Fields["BytesCount"] = new DefField { Value = 0, Quality = 192 };
				}
				UpdateEvent();

				return true;
            }
            catch (Exception e)
			{
                return Err("Ошибка при инициализации: " + e.Message);
            }
        }

        void CheckConnection()
        {
            bool isResetDoneRight = true;

            if (!IsActive) return;

            // Проверяем состояние подключения. Если в порядке, выходим
            if (Conn == null)
            {
                if (ConnectionPreviousStatus)
                {
                    LogEvent("Нет подключения", LogType.ERROR);
                    ConnectionPreviousStatus = false;
                }
                isResetDoneRight = ResetConnection();
            }

            if (!Conn.IsRunning)
            {
                if (ConnectionPreviousStatus)
                {
                    LogEvent("Подключение не активно", LogType.ERROR);
                    ConnectionPreviousStatus = false;
                }
                isResetDoneRight = ResetConnection();
            }

            if (isResetDoneRight && !ConnectionPreviousStatus)
            {
                LogEvent("Подключение активно", LogType.REGULAR);
                ConnectionPreviousStatus = true;
                Task.Run(SendInterrogation);
            }


            // Если подключения нет, выполняем сброс и реконнект
            //if (ConnectionPreviousStatus)
            //{
            //    ClearFields();
            //    LogEvent("Нет подключения", LogType.ERROR);
            //    ConnectionPreviousStatus = false;
            //}
            //try { Conn.Close(); } catch { }
            //try { Conn.Connect(); } catch { }
            //Conn.Close(); Conn.Connect();

            //if (Conn.IsRunning)
            //{
            //    LogEvent("Подключение установлено");
            //    Task.Run(SendInterrogation);
            //    ConnectionPreviousStatus = true;

            //    lock (Fields)
            //    {
            //        Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
            //        Fields["Connection"] = Conn.IsRunning;
            //        UpdateEvent();
            //    }
            //}
        }

        void SendInterrogation()
        {
            if (!IsActive) return;

            try
            {
                //if (Conn.IsRunning)
                //{
                    if (Configuration.UseInterrogation)
                    {
                        Conn.SendInterrogationCommand(CauseOfTransmission.ACTIVATION, 1, 20);
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " > INTERROGATION: Cot [" + 6 + "] Ca [" + 1 + "] Qoi [" + 20 + "]");
                    }
                    else
                    {
                        foreach (var field in Configuration.NamedFields)
                        {
                            Conn.SendReadCommand(1, field.Address);
                            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " > READ: Ca [" + 1 + "] Ioa [" + field.Address + "]");
                        }
                    }
                //}
            }
            catch (Exception e)
			{
                Err("Ошибка при опросе: " + e.Message);
            }
        }

        void SyncClock()
        {
            if (!IsActive) return;
            try
            {
                if (Conn.IsRunning)
                {
                    Conn.SendClockSyncCommand(1, new CP56Time2a(DateTime.Now));
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " > CLOCK: Ca [" + 1 + "]");
                }
            }
            catch (Exception e)
			{
                Err("Ошибка при синхронизации времени: " + e.Message);
            }
        }

        void ClearFields()
        {
            lock (Fields)
            {
                foreach (var field in Fields.Keys.ToArray())
                {
                    if (!new[] { "Time", "Connection", "BytesCount" }.Contains(field))
                    { 
                        Fields[field].Quality = 0;
                    }
                }
            }

            Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
            Fields["Connection"].Value = Conn.IsRunning;

            UpdateEvent();
        }

        long BytesCount = 0;

        bool RawMessageHandler(object parameter, byte[] message, int messageSize)
        {
            if (!IsActive) return false;

            if (BytesCount + messageSize > long.MaxValue)
            {
                BytesCount = 0;
            }
            else
            {
                BytesCount += messageSize;
            }

            LogEvent(parameter + ": " + Helpers.BytesToString(message.Take(messageSize).ToArray()), LogType.DETAILED);
            //Console.WriteLine("\t" + Helpers.BytesToString(message.Take(messageSize).ToArray()));

            return true;
        }

        bool AsduReceivedHandler(object parameter, ASDU asdu)
        {
            if (!IsActive) return false;

            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " < ASDU: Ca [" + asdu.Ca + "] Cot [" + asdu.Cot + "] Oa [" + asdu.Oa + "]");

            try
            {
                if (asdu.TypeId == TypeID.M_ME_TC_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueShortWithCP24Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }
                }
                else if (asdu.TypeId == TypeID.M_ME_TE_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueScaledWithCP56Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.ScaledValue);
                    }
                }
                else if (asdu.TypeId == TypeID.M_ME_TF_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueShortWithCP56Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }
                }

                else if (asdu.TypeId == TypeID.M_ME_NB_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueScaled)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.ScaledValue);
                    }
                }
                else if (asdu.TypeId == TypeID.M_ME_NC_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueShort)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }
                }
                else if (asdu.TypeId == TypeID.M_ME_ND_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueNormalizedWithoutQuality)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.NormalizedValue);
                    }
                }

                else if (asdu.TypeId == TypeID.M_SP_NA_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (SinglePointInformation)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }
                }
                else if (asdu.TypeId == TypeID.M_SP_TB_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (SinglePointWithCP56Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }
                }

                else if (asdu.TypeId == TypeID.C_SC_NA_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (SingleCommand)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.State);
                    }
                }
                else if (asdu.TypeId == TypeID.C_CS_NA_1)
                {
                    LogEvent("Получена команда синхронизации времени", LogType.WARNING);
                }
                else if (asdu.TypeId == TypeID.C_IC_NA_1)
                {
                    LogEvent("Получена команда опроса", LogType.WARNING);
                }
                else
                {
                    bool found = false;
                    foreach (var x in Enum.GetValues(typeof(TypeID)))
                    {
                        if ((int)asdu.TypeId == (int)x)
                        {
                            LogEvent("Получен известный тип данных, для которого не задан обработчик: " + asdu.TypeId, LogType.WARNING);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        LogEvent("Получен неизвестный тип данных, для которого не задан обработчик: " + asdu.TypeId, LogType.WARNING);
                    }
                }
            }
            catch (Exception e)
            {
                Err("Ошибка при обработке входящего ASDU" + e.Message);
            }

            lock (Fields)
            {
                Fields["BytesCount"].Value = BytesCount;
                Fields["Time"].Value = DateTime.Now.ToString("HH:mm:ss");
            }
            UpdateEvent();

            return true;
        }

        void WriteValue(int address, object value)
        {
            if (!IsActive) return;

            foreach (var field in Configuration.NamedFields)
            {
                if (field.Address == address)
                {
                    lock (Fields)
                    {
                        Fields[field.Name].Value = value;
                        Fields[field.Name].Quality = 192;
                    }
                    return;
                }
            }

            lock (Fields)
            {
                Fields["" + address].Value = new DefField
                {
                    Value = value,
                    Quality = 192
                };
            }
        }

        private bool Err(string text)
        {
            LogEvent(text, LogType.ERROR);
            return false;
        }
    }
}