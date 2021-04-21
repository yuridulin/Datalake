using System;

namespace iNOPC.Drivers.PR20_RS485.Models
{
	public class Field
    {
        public string Name { get; set; } = "";

        public object Value { get; set; } = 0;

        public string Type { get; set; } = "";

        public ushort Address { get; set; } = 0;

        public byte[] Command()
		{
            var address = BitConverter.GetBytes(Address);

            var command = new byte[]
            {
                // адрес
                0x01,
                // команда
                0x03,
                // 2 байта адреса
                address[1], address[0],
                // кол-во запрашиваемых байт
                0x00, 0x04,
            };

            var crc = BitConverter.GetBytes(CRC(command, 6));

            return new byte[]
            {
                // адрес
                0x01,
                // команда
                0x03,
                // 2 байта адреса
                address[1], address[0],
                // кол-во запрашиваемых байт
                0x00, 0x04,
                // контрольная сумма
                crc[0], crc[1],
            };

            ushort CRC(byte[] bytes, int len)
            {
                ushort _crc = 0xFFFF;
                for (int pos = 0; pos < len; pos++)
                {
                    _crc ^= bytes[pos];
                    for (int i = 8; i != 0; i--)
                    {
                        if ((_crc & 0x0001) != 0)
                        {
                            _crc >>= 1;
                            _crc ^= 0xA001;
                        }
                        else _crc >>= 1;
                    }
                }
                return _crc;
            }
        }

        public byte Length()
		{
            return 4 + 3;
		}
    }
}