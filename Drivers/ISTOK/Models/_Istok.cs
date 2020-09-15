using System;
using System.Runtime.InteropServices;

namespace ISTOK.Models
{
    public class Istok
    {
        [DllImport("IstokTM2.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern byte SetComPortConfig(ComPort NewComPortConfig, bool ReInit);

        [DllImport("IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte InitComPort();

        [DllImport("IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte FreeComPort();

        [DllImport("IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte GetComFuncError();


        [DllImport("IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadDateTime(byte nDevice, out DateTime aDateTime);

        [DllImport("IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadOperativeData(byte nDevice, out Operative aOperativeData);

        [DllImport("IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadHourData(byte nDevice, byte nPoint, DateTime aDateTime, out Archive aHourData);

        [DllImport("IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadDayData(byte nDevice, byte nPoint, DateTime aDateTime, out Archive aDayData);
    }
}