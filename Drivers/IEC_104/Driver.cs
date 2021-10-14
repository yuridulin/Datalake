using iNOPC.Library;
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
        public string Version { get; } = typeof(Driver).Assembly.GetName().Version.ToString();

        public Dictionary<string, DefField> Fields { get; set; } = new Dictionary<string, DefField>();

        public event LogEvent LogEvent;

        public event UpdateEvent UpdateEvent;

        public bool Start(string jsonConfig)
        {
            LogEvent("Запуск ...");

            // Очистка предыдущего подключения
            try { Conn?.Close(); } catch { }
            try { Conn = null; } catch { }
            ClearFields();

            // чтение конфигурации
            try
            {
                Configuration = JsonConvert.DeserializeObject<Configuration>(jsonConfig);
            }
            catch (Exception e)
            {
                return Err("Конфигурация не прочитана: " + e.Message);
            }

            // Создание нового подключения
            var apci = new APCIParameters
            {
                T0 = Configuration.ConnectionTimeoutT0, // задаётся в конфиге в секундах
                T1 = Configuration.TimeoutT1,
                T2 = Configuration.TimeoutT2,
                T3 = Configuration.TimeoutT3,
                K = 12,
                W = 8,
            };

            var alp = new ApplicationLayerParameters { };

            Conn = new Connection(Configuration.Host, Configuration.Port, apci, alp);
            Conn.SetConnectionHandler(ConnectionHandler, null);
            Conn.SetASDUReceivedHandler(AsduReceivedHandler, null);
            Conn.SetReceivedRawMessageHandler(RawMessageHandler, "RX");
            Conn.SetSentRawMessageHandler(RawMessageHandler, "TX");
            Conn.ReceiveTimeout = Configuration.ReceiveTimeout;

			// Создание полей, ранее заданных в конфиге
			Fields = new Dictionary<string, DefField>
			{
				["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 },
				["Connection"] = new DefField { Value = false, Quality = 192 },
				["BytesCount"] = new DefField { Value = 0, Quality = 192 }
			};

			try
            {
                foreach (var field in Configuration.NamedFields)
                {
                    switch (field.Type)
                    {
                        case "Bool":
                            Fields.Add(field.Name, new DefField { Value = false, Quality = 0 });
                            break;

                        case "Float":
                            Fields.Add(field.Name, new DefField { Value = 0F, Quality = 0 });
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                return Err("Ошибка при создании заданных полей: " + e.Message);
            }
            UpdateEvent();

            // Определение дополнительных таймеров
            InterrogationTimer = new Timer(Configuration.InterrogationDelay * 1000);
            InterrogationTimer.Elapsed += (s, e) =>
            {
                SendInterrogation();
            };

            ReconnectTimer = new Timer(Configuration.ReconnectTimeout * 1000);
            ReconnectTimer.Elapsed += (s, e) =>
            {
                LogEvent("Срабатывание таймера переподключения", LogType.WARNING);
                Reconnect();
            };

            // Запуск обмена данными
            IsActive = true;
            LogEvent("Мониторинг запущен");

            Task.Run(Reconnect);

            return true;
        }

        public void Stop()
        {
            LogEvent("Мониторинг останавливается...");

            IsActive = false;

            try { InterrogationTimer?.Stop(); } catch (Exception) { }
            try { InterrogationTimer = null; } catch (Exception) { }

            try { ReconnectTimer?.Stop(); } catch (Exception) { }
            try { ReconnectTimer = null; } catch (Exception) { }

            try { Conn?.Close(); } catch (Exception) { }
            try { Conn = null; } catch (Exception) { }

            ClearFields(true);

            LogEvent("Мониторинг остановлен");
        }

        public void Write(string fieldName, object value)
        {
            // Поиск адреса поля по имени в списке именованных полей
            int address = 0;
            string type = "bool";
            foreach (var field in Configuration.NamedFields)
            {
                if (field.Name == fieldName)
                {
                    address = field.Address;
                    type = field.Type;
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
                            switch (type)
							{
                                case "Bool":
                                    if (value.GetType().Equals(typeof(bool)))
									{
                                        Conn.SendControlCommand(CauseOfTransmission.ACTIVATION, 1, new SingleCommand(address, (bool)value, false, 0));
                                    }
                                    else if (bool.TryParse(value.ToString(), out bool v1))
                                    {
                                        Conn.SendControlCommand(CauseOfTransmission.ACTIVATION, 1, new SingleCommand(address, v1, false, 0));
                                    }
                                    else if (decimal.TryParse(value.ToString(), out decimal v2) && (v2 == 0 || v2 == 1))
									{
                                        Conn.SendControlCommand(CauseOfTransmission.ACTIVATION, 1, new SingleCommand(address, v2 != 0, false, 0));
                                    }
                                    else
									{
                                        LogEvent("Не удалось записать значение bool [" + fieldName + "], значение [" + value + "], значение как строка [" + value.ToString() + "], тип значения [" + value.GetType() + "]");
                                    }
                                    break;
                                case "Float":
                                    if (decimal.TryParse(value.ToString(), out decimal v3))
									{
                                        Conn.SendControlCommand(CauseOfTransmission.ACTIVATION, 1, new SetpointCommandShort(address, (float)v3, new SetpointCommandQualifier(0)));
                                        LogEvent("Успешная запись [" + fieldName + "], значение [" + value + "], тип значения [" + value.GetType() + "]");
                                    }
                                    else
									{
                                        LogEvent("Не удалось записать значение float [" + fieldName + "], значение [" + value + "], значение как строка [" + value.ToString() + "], тип значения [" + value.GetType() + "]");
                                    }
                                    break;
                                default:
                                    LogEvent("Не удалось определить команду на запись [" + fieldName + "], значение [" + value + "], значение как строка [" + value.ToString() + "], тип значения [" + value.GetType() + "]");
                                    break;
                            }

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
                }
            });
        }


        // Реализация получения данных

        Configuration Configuration { get; set; }

        Connection Conn { get; set; }

        Timer InterrogationTimer { get; set; }

        Timer ReconnectTimer { get; set; }

        bool IsActive { get; set; } = false;

        void ClearFields(bool all = false)
        {
            lock (Fields)
            {
                foreach (var field in Fields.Keys.ToArray())
                {
                    if (all || !new[] { "Time", "Connection", "BytesCount" }.Contains(field))
                    { 
                        Fields[field].Quality = 0;
                    }
                }
            }

            Fields["Time"] = new DefField { Value = DateTime.Now.ToString("HH:mm:ss"), Quality = 192 };
            Fields["Connection"] = new DefField { Value = Conn?.IsRunning ?? false, Quality = 192 };
            Fields["BytesCount"] = new DefField { Value = 0, Quality = 192 };

            UpdateEvent();
        }

        long BytesCount = 0;

        void Reconnect()
		{
            try { Conn.Connect(); } catch (Exception) { }
        }

        string LastConnectionEvent { get; set; }

        void ConnectionHandler(object parameter, ConnectionEvent connectionEvent)
		{
            LastConnectionEvent = connectionEvent.ToString();
            LogEvent("Состояние подключения: " + LastConnectionEvent, LogType.WARNING);

            if (!IsActive) return;

            if (LastConnectionEvent == "CONNECT_FAILED")
            {
                ClearFields();
                if (!ReconnectTimer.Enabled)
                {
                    LogEvent("Запуск таймера переподключения", LogType.WARNING);
                    ReconnectTimer.Start();
                    Fields["Connection"].Value = false;
                    UpdateEvent();
                }
            }

            if (LastConnectionEvent == "CLOSED")
            {
                ClearFields();
                if (!ReconnectTimer.Enabled)
                {
                    LogEvent("Запуск таймера переподключения", LogType.WARNING);
                    InterrogationTimer.Stop();
                    ReconnectTimer.Start();
                    Fields["Connection"].Value = false;
                    UpdateEvent();
                }
            }

            if (LastConnectionEvent == "OPENED")
            {
                if (ReconnectTimer.Enabled)
                {
                    LogEvent("Остановка таймера переподключения", LogType.WARNING);
                    ReconnectTimer.Stop();
                    Fields["Connection"].Value = true;
                    UpdateEvent();
                }
            }
        }

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

        void SendInterrogation()
		{
            if (!IsActive) return;
            LogEvent("Цикличный опрос", LogType.WARNING);

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

        bool AsduReceivedHandler(object parameter, ASDU asdu)
        {
            if (!IsActive) return false;

            try
            {
                // значения
                if (asdu.TypeId == TypeID.M_ME_TC_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueShortWithCP24Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }

                    LogEvent("Получено ASDU: M_ME_TC_1", LogType.WARNING);
                }
                else if (asdu.TypeId == TypeID.M_ME_TE_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueScaledWithCP56Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.ScaledValue);
                    }

                    LogEvent("Получено ASDU: M_ME_TE_1", LogType.WARNING);
                }
                else if (asdu.TypeId == TypeID.M_ME_TF_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueShortWithCP56Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }

                    LogEvent("Получено ASDU: M_ME_TF_1", LogType.WARNING);
                }
                else if (asdu.TypeId == TypeID.M_ME_NB_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueScaled)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.ScaledValue);
                    }

                    LogEvent("Получено ASDU: M_ME_NB_1", LogType.WARNING);
                }
                else if (asdu.TypeId == TypeID.M_ME_NC_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueShort)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }

                    LogEvent("Получено ASDU: M_ME_NC_1", LogType.WARNING);
                }
                else if (asdu.TypeId == TypeID.M_ME_ND_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (MeasuredValueNormalizedWithoutQuality)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.NormalizedValue);
                    }

                    LogEvent("Получено ASDU: M_ME_ND_1", LogType.WARNING);
                }
                else if (asdu.TypeId == TypeID.M_SP_NA_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (SinglePointInformation)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }

                    LogEvent("Получено ASDU: M_SP_NA_1", LogType.WARNING);
                }
                else if (asdu.TypeId == TypeID.M_SP_TB_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (SinglePointWithCP56Time2a)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.Value);
                    }

                    LogEvent("Получено ASDU: M_SP_TB_1", LogType.WARNING);
                }
                else if (asdu.TypeId == TypeID.C_SC_NA_1)
                {
                    for (int i = 0; i < asdu.NumberOfElements; i++)
                    {
                        var val = (SingleCommand)asdu.GetElement(i);
                        WriteValue(val.ObjectAddress, val.State);
                    }

                    LogEvent("Получено ASDU: C_SC_NA_1", LogType.WARNING);
                }

                // старт опроса
                else if (asdu.TypeId == TypeID.M_EI_NA_1)
				{
                    LogEvent("Инициализация окончена", LogType.WARNING);

                    InterrogationTimer.Start();
                    Task.Run(SendInterrogation);
                }

                // команды
                else if (asdu.TypeId == TypeID.C_CS_NA_1)
                {
                    LogEvent("Получена команда синхронизации времени", LogType.WARNING);
                }
                else if (asdu.TypeId == TypeID.C_IC_NA_1)
                {
                    //LogEvent("Получена команда опроса", LogType.WARNING);
                }

                // то, для чего обработчик не задан
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
                Fields["Connection"].Value = true;
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
                        Fields[field.Name] = new DefField
                        {
                            Value = value,
                            Quality = 192,
                        };
                    }
                    return;
                }
            }

            lock (Fields)
            {
                Fields["" + address] = new DefField
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