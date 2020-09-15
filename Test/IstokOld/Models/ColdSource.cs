using System.Runtime.InteropServices;

namespace Test.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ColdSource
    {
        public double t;

        public double P;

        public double Patm;

        public double h;
    }
}