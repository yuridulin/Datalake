using System.Runtime.InteropServices;

namespace Test.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Point
    {
        public float Q;

        public float G;

        public float h;

        public float dP;

        public float t;

        public float P;

        public float V;

        public float r;
    }
}