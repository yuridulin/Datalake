using System.Runtime.InteropServices;

namespace Test.Models
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct Operative
    {
        public ColdSource ColdSourceData;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public Point[] PointData;

        [MarshalAs(UnmanagedType.ByValArray, IidParameterIndex = 5, SizeConst = 16, ArraySubType = UnmanagedType.I8)]
        public double[] ChannelData;

        [MarshalAs(UnmanagedType.ByValArray, IidParameterIndex = 1, SizeConst = 4, ArraySubType = UnmanagedType.I8)]
        public double[] GroupDate;
    }
}