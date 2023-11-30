using System.Runtime.InteropServices;

namespace iNOPC.Drivers.IstokTM2
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct ComPort
	{
		public byte Number;

		public uint BaudRate;

		public byte StopBits;

		public byte Parity;

		public ushort Timeout;

		public byte CTS;

		public byte RTS;

		public byte DSR;

		public byte DTR;
	}
}