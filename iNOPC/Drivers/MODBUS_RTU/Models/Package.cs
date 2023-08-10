using iNOPC.Library;
using System;
using System.Collections;
using System.Collections.Generic;

namespace iNOPC.Drivers.MODBUS_RTU.Models
{
	public class Package
	{
		public byte SlaveId { get; set; } = 0;

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
			ReceiveLength = 5;
			foreach (PackagePart part in Parts) ReceiveLength += 2 * part.Length;


			// Составление команды на получение данных
			byte[] _address = BitConverter.GetBytes(StartAddress);

			byte[] command = new byte[]
			{
				// Slave ID (адрес устройства, может быть от 0 до 247)
				SlaveId,
				// команда на чтение Input Registers (полей с численными значениями)
				CommandCode,
				// стартовый адрес, начиная с которого будут забираться данные
				_address[1],
				_address[0],
				// кол-во байтов, которые необходимо забрать, зависит от типа забираемого значения и их кол-ва
				0,
				_length,
			};

			// контрольная сумма (CRC-16, записывается 2 байтами)
			byte[] _crc = BitConverter.GetBytes(CRC(command, 6));

			Trancieve = new byte[]
			{
				command[0], command[1], command[2], command[3], command[4], command[5],
				_crc[0], _crc[1],
			};
		}

		public string Receive(byte[] bytes)
		{
			string err = null;
			int position = 3;
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
						case "Date":
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

						case "TM2Date":
							value = new DateTime(valueBytes[5], valueBytes[4], valueBytes[3], valueBytes[2], valueBytes[1], valueBytes[0]).ToString("HH:mm:ss");
							break;

						case "Word":
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

							value = BitConverter.ToInt16(toValue, 0);
							break;

						case "UInt16":
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

							value = BitConverter.ToUInt16(toValue, 0);
							break;

						case "Int":
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

							value = BitConverter.ToInt32(toValue, 0);
							break;

						case "UInt32":
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

							value = BitConverter.ToUInt32(toValue, 0);
							break;

						case "Long":
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

							part.Value = BitConverter.ToInt64(toValue, 0);
							break;


						case "UInt64":
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

							value = BitConverter.ToUInt64(toValue, 0);
							break;

						case "Double":
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

							value = BitConverter.ToDouble(toValue, 0);
							break;

						case "Single":
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

							value = BitConverter.ToSingle(toValue, 0);
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

							value = Convert.ToDouble(first.ToString() + "," + second.ToString());
							break;

						case "AKVT_BCD":
							var b0 = new BitArray(new byte[] { OldByteFirst ? valueBytes[3] : valueBytes[0] });
							var b1 = new BitArray(new byte[] { OldByteFirst ? valueBytes[2] : valueBytes[1] });
							var b2 = new BitArray(new byte[] { OldByteFirst ? valueBytes[1] : valueBytes[2] });
							var b3 = new BitArray(new byte[] { OldByteFirst ? valueBytes[0] : valueBytes[3] });

							float bcd =
								GetBCD(new bool[] { b1[7], b1[6], b1[5], b1[4] }) * 100000 +
								GetBCD(new bool[] { b1[3], b1[2], b1[1], b1[0] }) * 10000 +
								GetBCD(new bool[] { b2[7], b2[6], b2[5], b2[4] }) * 1000 +
								GetBCD(new bool[] { b2[3], b2[2], b2[1], b2[0] }) * 100 + 
								GetBCD(new bool[] { b3[7], b3[6], b3[5], b3[4] }) * 10 +
								GetBCD(new bool[] { b3[3], b3[2], b3[1], b3[0] });

							if (b0[7]) bcd *= -1;
							if (b0[2]) bcd /= 10000;
							if (b0[1]) bcd /= 1000;
							if (b0[0]) bcd /= 100;

							value = bcd;
							break;
					}

					position += bytesCount;
				}
				catch (Exception ex)
				{
					err = ex.Message + "\n" + ex.StackTrace;
					value = part.Value;
				}

				part.Value = value;
			}

			return err;
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
			return crc;
		}

		static int GetBCD(bool[] bools)
		{
			if (!bools[0] && !bools[1] && !bools[2] && !bools[3]) return 0;
			if (!bools[0] && !bools[1] && !bools[2] &&  bools[3]) return 1;
			if (!bools[0] && !bools[1] &&  bools[2] && !bools[3]) return 2;
			if (!bools[0] && !bools[1] &&  bools[2] &&  bools[3]) return 3;
			if (!bools[0] &&  bools[1] && !bools[2] && !bools[3]) return 4;
			if (!bools[0] &&  bools[1] && !bools[2] &&  bools[3]) return 5;
			if (!bools[0] &&  bools[1] &&  bools[2] && !bools[3]) return 6;
			if (!bools[0] &&  bools[1] &&  bools[2] &&  bools[3]) return 7;
			if ( bools[0] && !bools[1] && !bools[2] && !bools[3]) return 8;
			if ( bools[0] && !bools[1] && !bools[2] &&  bools[3]) return 9;
			return 0;
		}
	}
}