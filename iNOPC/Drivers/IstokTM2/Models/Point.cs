using System.Runtime.InteropServices;

namespace iNOPC.Drivers.IstokTM2
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct Point
	{
		public double Q;

		public double G;

		public double h;

		public double dP;

		public double t;

		public double P;

		public double V;

		public double r;
	}
}