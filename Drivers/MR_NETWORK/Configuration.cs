using iNOPC.Drivers.MR_NETWORK.Models;
using iNOPC.Library;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace iNOPC.Drivers.MR_NETWORK
{
    public class Configuration
    {
        public int Timeout { get; set; }

        public string PortName { get; set; }

        public int BaudRate { get; set; }

        public int DataBits { get; set; }

        public byte Parity { get; set; }

        public byte StopBits { get; set; }

        public byte DeviceNumber { get; set; }

        public int FieldsInterval { get; set; }

        public int ReceiveTimeout { get; set; }

        public List<Field> Fields { get; set; } = new List<Field>();

        public static string GetPage(string json)
        {
            var config = JsonConvert.DeserializeObject<Configuration>(json);

            return
                Html.Value("Интервал опроса, мс", nameof(config.Timeout), config.Timeout) +
                Html.Value("COM порт", nameof(config.PortName), config.PortName) +
                Html.Value("Скорость", nameof(config.BaudRate), config.BaudRate) +
                Html.Value("Кол-во бит данных", nameof(config.DataBits), config.DataBits) +
                Html.Value("Четность (1 = исп.)", nameof(config.Parity), config.Parity) +
                Html.Value("Кол-во стоповых бит", nameof(config.StopBits), config.StopBits) +
                Html.Value("Адрес устройства", nameof(config.DeviceNumber), config.DeviceNumber) +
                Html.Value("Таймаут на запись, мс", nameof(config.FieldsInterval), config.FieldsInterval) +
                Html.Value("Таймаут на чтение, мс", nameof(config.ReceiveTimeout), config.ReceiveTimeout);
        }
    }
}