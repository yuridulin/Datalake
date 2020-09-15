using iNOPC.Library;
using lib60870;
using lib60870.CS101;
using lib60870.CS104;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Drivers.IEC_104
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
            Fields = new Dictionary<string, object>();

            // Очистка предыдущего подключения
            try { Conn?.Close(); Conn = null; } catch { }
            LostConnection = true;

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
            try
            {
                foreach (var field in Configuration.NamedFields)
                {
                    Fields.Add(field.Name, 0);
                }
            }
            catch (Exception e)
            {
                return Err("Ошибка при создании заданных полей: " + e.Message);
            }

            // запуск опроса
            try
            {
                Conn = new Connection(Configuration.Host, Configuration.Port)
                {
                    DebugOutput = false,
                    Autostart = true,
                };

                Conn.SetConnectTimeout(Configuration.ConnectionTimeout);
                Conn.SetASDUReceivedHandler(AsduReceivedHandler, null);
                Conn.SetReceivedRawMessageHandler(RawMessageHandler, "RX");
                Conn.SetSentRawMessageHandler(RawMessageHandler, "TX");
            }
            catch (Exception e)
            {
                return Err("Подключение не создано: " + e.Message);
            }

            lock (Fields)
            {
                Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
                Fields["Connection"] = false;
            }
            UpdateEvent();

            IsActive = true;

            ConnectionTimer = new Timer(Configuration.ReconnectTimeout);
            ConnectionTimer.Elapsed += (s, e) => CheckConnection();
            ConnectionTimer.Start();

            InterrogationTimer = new Timer(Configuration.InterrogationTimeout);
            InterrogationTimer.Elapsed += (s, e) => SendInterrogation();
            InterrogationTimer.Start();

            ClockTimer = new Timer(Configuration.SyncClockTimeout);
            ClockTimer.Elapsed += (s, e) => SyncClock();
            ClockTimer.Start();

            Task.Run(CheckConnection);

            LogEvent("Мониторинг запущен");
            return true;
        }

        public void Stop()
        {
            LogEvent("Мониторинг останавливается...");

            IsActive = false;

            ConnectionTimer.Stop();
            InterrogationTimer.Stop();
            ClockTimer.Stop();
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
            if (Conn.IsRunning)
            {
                try
                {
                    bool command = true;

                    if (value.GetType().Equals(typeof(bool)))
                    {
                        command = (bool)value;
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

                    Conn.SendControlCommand(CauseOfTransmission.ACTIVATION, 1, new SingleCommand(address, command, false, 0));

                    LogEvent("Успешная запись [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
                    WinLogEvent("Успешная запись [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
                }
                catch (Exception e)
                {
                    Err("Ошибка при записи: " + e.Message);
                }
            }
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        Timer ConnectionTimer { get; set; }

        Timer InterrogationTimer { get; set; }

        Timer ClockTimer { get; set; }

        Connection Conn { get; set; }

        bool LostConnection { get; set; }

        bool IsActive { get; set; } = false;

        void CheckConnection()
        {
            if (!IsActive) return;

            if (Conn.IsRunning)
            {
                ChangeToOnline();
                return;
            }

            try { Conn.Close(); } catch { }
            try 
            { 
                Conn.Connect();

                if (Conn.IsRunning)
                {
                    ChangeToOnline();
                }
            } 
            catch (Exception e)
            {
                ChangeToOffline(e);
            }

            void ChangeToOnline()
            {
                if (LostConnection)
                {
                    LostConnection = false;
                    LogEvent("Подключение установлено");
                    Task.Run(SendInterrogation);

                    lock (Fields)
                    {
                        Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
                        Fields["Connection"] = Conn.IsRunning;
                        UpdateEvent();
                    }
                }
            }

            void ChangeToOffline(Exception e)
            {
                if (!LostConnection)
                {
                    LostConnection = true;
                    ClearFields();
                    LogEvent("Подключение не установлено: " + e.Message, LogType.ERROR);
                }
            }
        }

        void SendInterrogation()
        {
            if (!IsActive) return;

            if (Conn.IsRunning)
            {
                if (Configuration.UseInterrogation)
                { 
                    Conn.SendInterrogationCommand(CauseOfTransmission.ACTIVATION, 1, 20);
                }
                else
                {
                    foreach (var field in Configuration.NamedFields)
                    {
                        Conn.SendReadCommand(1, field.Address);
                    }
                }
            }
        }

        void SyncClock()
        {
            if (!IsActive) return;

            if (Conn.IsRunning)
            {
                Conn.SendClockSyncCommand(1, new CP56Time2a(DateTime.Now));
            }
        }

        void ClearFields()
        {
            lock (Fields)
            {
                foreach (var field in Fields.Keys.ToArray())
                {
                    if (field != "Time" && field != "Connection") Fields[field] = null;
                }
            }

            Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
            Fields["Connection"] = Conn.IsRunning;

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

            return true;
        }

        bool AsduReceivedHandler(object parameter, ASDU asdu)
        {
            if (!IsActive) return false;

            try
            {
                if (asdu.TypeId == TypeID.M_SP_NA_1) /* Однобитная информация в байте (ТС) */
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (SinglePointInformation)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }
                }
                else if (asdu.TypeId == TypeID.M_ME_TE_1) /* Значение измеряемой величины, масштабированное значение (2 байта) с меткой времени (7 байт) */
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueScaledWithCP56Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.ScaledValue);
                    }
                }
                else if (asdu.TypeId == TypeID.M_ME_TF_1) /* Значение измеряемой величины, короткий формат с плавающей запятой (4 байта) с меткой времени */
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueShortWithCP56Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }
                }
                else if (asdu.TypeId == TypeID.M_SP_TB_1) /* Однобитная информация в байте (ТС) c меткой времени (7 байт) */
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (SinglePointWithCP56Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }
                }
                else if (asdu.TypeId == TypeID.M_ME_NC_1) /* Значение измеряемой величины, короткий формат с плавающей запятой (4 байта) */
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueShort)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }
                }
                else if (asdu.TypeId == TypeID.M_ME_NB_1) /* Значение измеряемой величины, масштабированное значение (2 байта) */
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueScaled)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.ScaledValue);
                    }
                }
                else if (asdu.TypeId == TypeID.M_ME_ND_1) /* Значение измеряемой величины, нормализованное значение (2 байта) без описателя качества */
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueNormalizedWithoutQuality)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.NormalizedValue);
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
            catch { }

            lock (Fields)
            {
                Fields["BytesCount"] = BytesCount;
                Fields["Time"] = DateTime.Now.ToString("HH:mm:ss");
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
                        Fields[field.Name] = value;
                    }
                    return;
                }
            }

            lock (Fields)
            {
                Fields["" + address] = value;
            }
        }

        private bool Err(string text)
        {
            LogEvent(text, LogType.ERROR);
            return false;
        }
    }
}