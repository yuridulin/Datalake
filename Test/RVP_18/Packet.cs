using System.Collections.Generic;

namespace RVP_18
{
	public class Packet
	{
		public byte[] Bytes { get; set; }

		public Packet()
		{
			Bytes = new byte[]
			{
				// Маркер начала пакета
				0x17, 0x13,

				// Длина пакета в байтах без учёта маркеров начала и конца (вместе с полем длины и CRC)
				0x00, 0x00,

				// Номер прибора
				0x00, 0x00, 0x00,

				// Тип пакета (типизированы как enum PacketType, от типа зависит структура данных пакета)
				0x00,

				// Данные

				// CRC (вычисляется из всего, кроме маркеров)
				0x00, 0x00,

				// Маркер конца пакета
				0x17, 0x05,
			};
		}

		public void CreateEventPacket()
		{
			var data = new List<byte>();
		}
	}

	public enum PacketType
	{
		Event = 0,
		Confirmation = 1,
		Text = 2,
		State = 3,
	}
}
