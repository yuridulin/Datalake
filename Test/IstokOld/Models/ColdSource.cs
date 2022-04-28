using System.Runtime.InteropServices;

namespace Test.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct ColdSource
    {
        public float t;

        public float P;

        public float Patm;

        public float h;
    }
}