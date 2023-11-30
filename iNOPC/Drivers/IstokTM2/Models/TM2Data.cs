using System.Runtime.InteropServices;

namespace iNOPC.Drivers.IstokTM2
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct TM2Data
	{
		public float nRec;

		public double DateTime;

		[MarshalAs(UnmanagedType.ByValArray, IidParameterIndex = 1, SizeConst = 13)]
		public Data[] Data;
	}
}