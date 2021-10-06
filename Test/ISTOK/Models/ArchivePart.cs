using System.Runtime.InteropServices;

namespace ISTOK.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ArchivePart
    {
        public double t;

        public double P;

        public double Patm;

        public double Q;

        public double G;

        public double M;
    }
}