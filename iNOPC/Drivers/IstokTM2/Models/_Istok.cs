using System;
using System.Runtime.InteropServices;

namespace iNOPC.Drivers.IstokTM2
{
    public class Istok
    {
        [DllImport("Drivers\\IstokDll\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern byte GetComPortConfig(out ComPort CurrComPortConfig);

        [DllImport("Drivers\\IstokDll\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern byte SetComPortConfig(ComPort NewComPortConfig, bool ReInit);

        [DllImport("Drivers\\IstokDll\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte InitComPort();

        [DllImport("Drivers\\IstokDll\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte FreeComPort();

        [DllImport("Drivers\\IstokDll\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte GetComFuncError();

        [DllImport("Drivers\\IstokDll\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadDateTime(byte nDevice, out double aDateTime);

        [DllImport("Drivers\\IstokDll\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadOperativeData(byte nDevice, out Operative aOperativeData);

        [DllImport("Drivers\\IstokDll\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadHourData(byte nDevice, byte nPoint, double aDateTime, out TM2Data aHourData);

        [DllImport("Drivers\\IstokDll\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadDayData(byte nDevice, byte nPoint, double aDateTime, out TM2Data aDayData);
    }
}