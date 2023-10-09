using System;
using System.Collections.Generic;
using System.IO;

namespace iNOPC.Drivers.MT01
{
	public class Protocol
	{
		public enum TypeArch
		{
			min = 15,
			hour,
			day,
			mon,
			year
		}

		public Protocol()
		{
		}

		public Protocol(byte addr, int tmout)
		{
			_adressDevice = addr;
			_timeout = tmout;
		}


		private byte _adressDevice = 0;
		private int _timeout;

		private Stream _stream;
		public Stream ConStream { get => _stream; set => _stream = value; }


		public struct Arch_t
		{
			public int second;
			public int minutes;
			public int hours;
			public int days;
			public int month;
			public int years;
			public int value;

			static public int Lenght() //Длина среда в архиве счетчика 10 байт!!!
			{
				return 10;
			}
		}

		private readonly byte[] crc8Table =
		{
	  0x31, 0x07, 0x0E, 0x09, 0x1C, 0x1B, 0x12, 0x15, 0x38, 0x3F, 0x36, 0x31, 0x24, 0x23, 0x2A, 0x2D,
	  0x70, 0x77, 0x7E, 0x79, 0x6C, 0x6B, 0x62, 0x65, 0x48, 0x4F, 0x46, 0x41, 0x54, 0x53, 0x5A, 0x5D,
	  0xE0, 0xE7, 0xEE, 0xE9, 0xFC, 0xFB, 0xF2, 0xF5, 0xD8, 0xDF, 0xD6, 0xD1, 0xC4, 0xC3, 0xCA, 0xCD,
	  0x90, 0x97, 0x9E, 0x99, 0x8C, 0x8B, 0x82, 0x85, 0xA8, 0xAF, 0xA6, 0xA1, 0xB4, 0xB3, 0xBA, 0xBD,
	  0xC7, 0xC0, 0xC9, 0xCE, 0xDB, 0xDC, 0xD5, 0xD2, 0xFF, 0xF8, 0xF1, 0xF6, 0xE3, 0xE4, 0xED, 0xEA,
	  0xB7, 0xB0, 0xB9, 0xBE, 0xAB, 0xAC, 0xA5, 0xA2, 0x8F, 0x88, 0x81, 0x86, 0x93, 0x94, 0x9D, 0x9A,
	  0x27, 0x20, 0x29, 0x2E, 0x3B, 0x3C, 0x35, 0x32, 0x1F, 0x18, 0x11, 0x16, 0x03, 0x04, 0x0D, 0x0A,
	  0x57, 0x50, 0x59, 0x5E, 0x4B, 0x4C, 0x45, 0x42, 0x6F, 0x68, 0x61, 0x66, 0x73, 0x74, 0x7D, 0x7A,
	  0x89, 0x8E, 0x87, 0x80, 0x95, 0x92, 0x9B, 0x9C, 0xB1, 0xB6, 0xBF, 0xB8, 0xAD, 0xAA, 0xA3, 0xA4,
	  0xF9, 0xFE, 0xF7, 0xF0, 0xE5, 0xE2, 0xEB, 0xEC, 0xC1, 0xC6, 0xCF, 0xC8, 0xDD, 0xDA, 0xD3, 0xD4,
	  0x69, 0x6E, 0x67, 0x60, 0x75, 0x72, 0x7B, 0x7C, 0x51, 0x56, 0x5F, 0x58, 0x4D, 0x4A, 0x43, 0x44,
	  0x19, 0x1E, 0x17, 0x10, 0x05, 0x02, 0x0B, 0x0C, 0x21, 0x26, 0x2F, 0x28, 0x3D, 0x3A, 0x33, 0x34,
	  0x4E, 0x49, 0x40, 0x47, 0x52, 0x55, 0x5C, 0x5B, 0x76, 0x71, 0x78, 0x7F, 0x6A, 0x6D, 0x64, 0x63,
	  0x3E, 0x39, 0x30, 0x37, 0x22, 0x25, 0x2C, 0x2B, 0x06, 0x01, 0x08, 0x0F, 0x1A, 0x1D, 0x14, 0x13,
	  0xAE, 0xA9, 0xA0, 0xA7, 0xB2, 0xB5, 0xBC, 0xBB, 0x96, 0x91, 0x98, 0x9F, 0x8A, 0x8D, 0x84, 0x83,
	  0xDE, 0xD9, 0xD0, 0xD7, 0xC2, 0xC5, 0xCC, 0xCB, 0xE6, 0xE1, 0xE8, 0xEF, 0xFA, 0xFD, 0xF4, 0xF3
	};

		private byte crc8tab(byte[] pblock, int len)
		{
			byte crc = 0x00;

			for (int i = 0; i < len; i++)
				crc = crc8Table[crc ^ pblock[i]];

			return crc;
		}

		private byte[] PrepareRequest(byte func, byte cmd, UInt32 data)
		{
			byte[] dataByte = BitConverter.GetBytes(data);

			byte[] Txdata = new byte[8] { _adressDevice, func, cmd, dataByte[0], dataByte[1], dataByte[2], dataByte[3], 0 };
			byte crc = crc8tab(Txdata, Txdata.Length - 1);
			Txdata[Txdata.Length - 1] = crc;
			return Txdata;
		}

		private byte[] ReadData(byte[] buffer, out int lenght, int forRcv)//TODO зачем здесь вообще длина?
		{
			_stream.Write(buffer, 0, buffer.Length);
			byte[] buf = new byte[256];
			int x = ReadAll(buf, 0, forRcv);
			if (x == -1)
				throw new Exception("Ошибка считывания...");
			byte crc = crc8tab(buf, x - 1);
			if (crc != buf[x - 1])
				throw new Exception("Ошибка CRC");
			lenght = x - 4;
			byte[] retbuf = new byte[x - 4];
			Array.Copy(buf, 3, retbuf, 0, retbuf.Length);
			return retbuf;
		}


		private int ReadAll(byte[] buffer, int offset, int count)
		{
			int realRead = 0;
			try
			{
				while (count > realRead)
				{
					var aResult = _stream.BeginRead(buffer, realRead + offset, count - realRead, null, null);
					var res = aResult.AsyncWaitHandle.WaitOne(_timeout);
					if (!res)
					{
						throw new TimeoutException();
					}
					else realRead += _stream.EndRead(aResult);
					if (realRead == 5 && ((buffer[1] & 0x80) != 0))
					{
						return realRead;
					}
				}
			}
			catch (TimeoutException)
			{
				throw;
			}
			catch (Exception exp)
			{
				return -1;
			}

			return realRead;

		}


		//TODO переделать в CleanAllEepromCuts
		public void CleanMeter(System.IO.Stream stream)
		{
			//TODO!
			//byte[] bytes = PrepareRequest_old(adressDevice, 202, 0);
			//int len = 0;
			//byte[] buf = ReadData(bytes, out len, true, stream);
			//if (buf[3] == 202)
			//{
			//  MessageBox.Show("Память очищена успешно", "Очистка", MessageBoxButton.OK, MessageBoxImage.Information);
			//}
			//else
			//{
			//  MessageBox.Show("Не удалось выполнить команду", "Очистка", MessageBoxButton.OK, MessageBoxImage.Error);
			//}
		}
		//TODO! Убрать
		public void SetMeterCLBN(bool clbn)
		{
		}
		//TODO! Убрать
		public void SetMeterUpdate()
		{
		}
		//TODO! А надо ли это здесь
		public void SetMeterReboot()
		{
		}

		public int GetMetherEnergy()
		{
			byte[] bytes = PrepareRequest(1, 1, 0);
			int len = 0;
			byte[] buf = ReadData(bytes, out len, 8);
			int power = BitConverter.ToInt32(buf, 0);
			return power;
		}

		public int[] GetMetherPow()
		{
			byte[] bytes = PrepareRequest(1, 2, 0);
			int len = 0;
			byte[] buf = ReadData(bytes, out len, 28);
			int[] power = new int[6];
			for (int i = 0; i < 6; i++)
				power[i] = BitConverter.ToInt32(buf, i * 4);
			return power;
		}

		public int[] GetMetherU()
		{
			byte[] bytes = PrepareRequest(1, 3, 0);
			int lens = 0;
			byte[] dataByte = ReadData(bytes, out lens, 16);
			byte len = 3;
			int[] val = new int[len];
			for (int i = 0; i < len; i++)
				val[i] = BitConverter.ToInt32(dataByte, i * 4);
			return val;
		}

		public int[] GetMetherI()
		{
			byte[] bytes = PrepareRequest(1, 4, 0);
			int lens = 0;
			byte[] dataByte = ReadData(bytes, out lens, 16);
			byte len = 3;
			int[] val = new int[len];
			for (int i = 0; i < len; i++)
				val[i] = BitConverter.ToInt32(dataByte, i * 4);
			return val;
		}

		public int[] GetMetherCos()
		{
			byte[] bytes = PrepareRequest(1, 5, 0);
			int lens = 0;
			byte[] dataByte = ReadData(bytes, out lens, 16);
			byte len = 3;
			int[] val = new int[len];
			for (int i = 0; i < len; i++)
				val[i] = BitConverter.ToInt32(dataByte, i * 4);
			return val;
		}

		public int[] GetMetherF()
		{
			byte[] bytes = PrepareRequest(1, 6, 0);
			int lens = 0;
			byte[] dataByte = ReadData(bytes, out lens, 16);
			byte len = 3;
			int[] val = new int[len];
			for (int i = 0; i < len; i++)
				val[i] = BitConverter.ToInt32(dataByte, i * 4);
			return val;
		}

		public string GetMetherType()
		{
			byte[] bytes = PrepareRequest(1, 7, 0);
			int len = 0;
			byte[] buf = ReadData(bytes, out len, 8);
			string txt = System.Text.ASCIIEncoding.ASCII.GetString(buf, 0, len);
			return txt;
		}

		public int GetSerialNum()
		{
			byte[] bytes = PrepareRequest(1, 8, 0);
			int len = 0;
			byte[] buf = ReadData(bytes, out len, 8);
			return BitConverter.ToInt32(buf, 0);
		}

		public DateTime GetMetherDateOfIssue()
		{
			byte[] bytes = PrepareRequest(1, 9, 0);
			int len = 0;
			byte[] buf = ReadData(bytes, out len, 8);
			DateTime dat = new DateTime(buf[3] + 2000, buf[2], buf[1]);
			return dat;
		}

		public string GetMetherVersion()
		{
			byte[] bytes = PrepareRequest(1, 10, 0);
			int len = 0;
			byte[] dataByte = ReadData(bytes, out len, 8);
			string str = $"{dataByte[0]}.{dataByte[1]}.{dataByte[2]}.{dataByte[3]}";
			return str;
		}

		public int GetMetherNetAdr()
		{
			byte[] bytes = PrepareRequest(1, 11, 0);
			int len = 0;
			byte[] dataByte = ReadData(bytes, out len, 8);
			return dataByte[0];
		}


		public int[] GetUARTConfig()
		{
			byte[] bytes = PrepareRequest(1, 12, 0);
			int lens = 0;
			byte[] dataByte = ReadData(bytes, out lens, 8);
			byte len = 3;
			int[] val = new int[len];
			val[0] = BitConverter.ToInt16(dataByte, 0) * 100;
			val[1] = dataByte[2];
			val[2] = dataByte[3];
			return val;
		}

		public int GetPulseConst()
		{
			byte[] bytes = PrepareRequest(1, 13, 0);
			int len = 0;
			byte[] dataByte = ReadData(bytes, out len, 8);
			return BitConverter.ToInt32(dataByte, 0);
		}

		public DateTime GetDateTime()
		{
			byte[] bytes = PrepareRequest(1, 14, 0);
			int len = 0;
			byte[] dataByte = ReadData(bytes, out len, 8);
			UInt32 t = BitConverter.ToUInt32(dataByte, 0);
			DateTime dat = new DateTime(1970, 1, 1).AddSeconds(BitConverter.ToInt32(dataByte, 0));
			return dat;
		}

		public List<Arch_t> GetMetherLog()
		{
			List<Arch_t> log = new List<Arch_t>();

			//      byte[] buf = PrepareRequest_old(adressDevice, 20, 0);
			//      List<byte[]> lst = ReadDataArcive(buf, stream);
			//      for (int i = 0; i < lst.Count; i++)
			//      {
			//        byte[] dataByte = new byte[lst[i][2]];
			//        Array.Copy(lst[i], 3, dataByte, 0, dataByte.Length);
			//#if false
			//        for (int n = 0; n < dataByte.Length; n += 5)
			//        {
			//          Arch_t arc = new Arch_t() { dateTime = BitConverter.ToInt32(dataByte, n), value = dataByte[n + 4] };
			//#else
			//        for (int n = 0; n < dataByte.Length; n += 7)
			//        {
			//          Arch_t arc = new Arch_t()
			//          {
			//            second = dataByte[n],
			//            minutes = dataByte[n+1],
			//            hours = dataByte[n+2],
			//            days = dataByte[n+3],
			//            month = dataByte[n+4],
			//            years = dataByte[n+5] + 2000,
			//            value = dataByte[n + 6]
			//          };
			//#endif
			//          //arc.dateTime = BitConverter.ToInt32(dataByte, i);
			//          //arc.value = BitConverter.ToInt32(dataByte, i + 4);
			//          log.Add(arc);
			//        }
			//      }
			return log;
		}

		public List<Arch_t> GetHalfHourCut(DateTime start)
		{
			List<Arch_t> arch = new List<Arch_t>();

			byte[] dtt = new byte[4];

			dtt[0] = (byte)start.Day;
			dtt[1] = (byte)start.Month;
			dtt[2] = (byte)(start.Year % 100);
			dtt[3] = (byte)(start.Hour / 12);

			byte[] bytes = PrepareRequest(1, 16, BitConverter.ToUInt32(dtt, 0));
			int len = 0;
			byte[] dataByte = ReadData(bytes, out len, 244);

			if (dataByte.Length == 1)
			{
				//Is Error
				//if(dataByte[0] == 246)
				switch (dataByte[0])
				{
					case 246:
						throw new Exception("Ошибка: слишком раняя дата для архива");
					case 247:
						throw new Exception("Ошибка: неправильная дата");
				}
			}

			for (int i = 0; i < dataByte.Length; i += 10)
			{
				Arch_t arc = new Arch_t()
				{
					second = dataByte[i],
					minutes = dataByte[i + 1],
					hours = dataByte[i + 2],
					days = dataByte[i + 3],
					month = dataByte[i + 4],
					years = dataByte[i + 5] + 2000,
					value = BitConverter.ToInt32(dataByte, i + 6)
				};
				arch.Add(arc);
			}
			return arch;
		}

		public List<Arch_t> GetCut(byte cmd, UInt32 date)
		{
			List<Arch_t> arch = new List<Arch_t>();
			byte[] bytes = PrepareRequest(1, cmd, date);
			int len = 0;
			byte[] dataByte = ReadData(bytes, out len, 244);
			int count = 0;
			if (dataByte.Length == 1) //error
			{
				switch (dataByte[0])
				{
					case 252:
						//MessageBox.Show("Архив пуст!", "Чтение архивов", MessageBoxButton.OK, MessageBoxImage.Warning);
						return null;
				}
			}
			for (int i = 0; i < dataByte.Length; i += 10)
			{
				Arch_t arc = new Arch_t()
				{
					second = dataByte[i],
					minutes = dataByte[i + 1],
					hours = dataByte[i + 2],
					days = dataByte[i + 3],
					month = dataByte[i + 4],
					years = dataByte[i + 5] + 2000,
					value = BitConverter.ToInt32(dataByte, i + 6)
				};

				if (arc.month == 0 && arc.days == 0)
				{
					count = i + 1;
					break;
				}

				arch.Add(arc);
			}

			return arch;
		}

		public bool SetUARTConfig(UInt16 Baudrate, byte Parity, byte StopBits)
		{
			byte[] data = new byte[4] {
		BitConverter.GetBytes(Baudrate)[0],
		BitConverter.GetBytes(Baudrate)[1],
		Parity,
		StopBits};
			UInt32 v = BitConverter.ToUInt32(data, 0);
			byte[] bytes = PrepareRequest(2, 12, v);
			int lens = 0;
			byte[] dataByte = ReadData(bytes, out lens, 8);

			return (dataByte[0] == 1);
		}

		public bool SetNetAdr(byte Address)
		{
			byte[] bytes = PrepareRequest(2, 11, Address);
			int len = 0;
			byte[] dataByte = ReadData(bytes, out len, 8);
			return (dataByte[0] == 1);
		}

		public bool EraceSeg(byte segmentType)
		{
			byte[] bytes = PrepareRequest(3, 244, segmentType);
			int len = 0;
			byte[] dataByte = ReadData(bytes, out len, 5);
			return (dataByte[0] == 1);
		}

		public int[] GetMaxPow(int phase)
		{
			int[] r = new int[0]; //заглушка нет ф-и на сервере
			return r;
		}

		public void TimeSynchronize()
		{
			int unixTime = (int)(DateTime.UtcNow.AddHours(3).AddSeconds(1) - new DateTime(1970, 1, 1)).TotalSeconds;
			byte[] bytes = PrepareRequest(2, 14, (UInt32)unixTime);
			int len = 0;
			byte[] dataByte = ReadData(bytes, out len, 8);
			if (dataByte[0] == 1)
			{
				//MessageBox.Show("Время синхронизировано успешно", "Синхронизация времени", MessageBoxButton.OK, MessageBoxImage.Information);
			}
			else
			{
				//MessageBox.Show("Не удалось синхронизировать время", "Синхронизация времени", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
	}
}
