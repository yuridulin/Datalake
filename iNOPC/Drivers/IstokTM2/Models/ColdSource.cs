using System.Runtime.InteropServices;

namespace iNOPC.Drivers.IstokTM2
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ColdSource
    {
        public double t;

        public double P;

        public double Patm;

        //public double h;
    }
}