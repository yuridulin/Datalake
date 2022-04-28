using System.Runtime.InteropServices;

namespace iNOPC.Drivers.IstokTM2
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Data
    {
        public double t;

        public double P;

        public double Patm;

        public double Q;

        public double G;

        public double M;
    }
}