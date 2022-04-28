using System;
using System.Runtime.InteropServices;

namespace Test.Models
{
    public class Istok
    {
        [DllImport("Models\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
        public static extern byte SetComPortConfig(ComPort NewComPortConfig, bool ReInit);

        [DllImport("Models\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte InitComPort();

        [DllImport("Models\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte FreeComPort();

        [DllImport("Models\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte GetComFuncError();


        [DllImport("Models\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadDateTime(byte nDevice, out double aDateTime);


        [DllImport("Models\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadOperativeData(byte nDevice, out Operative aOperativeData);

        [DllImport("Models\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadHourData(byte nDevice, byte nPoint, DateTime aDateTime, out Archive aHourData);

        [DllImport("Models\\IstokTM2.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern byte ReadDayData(byte nDevice, byte nPoint, DateTime aDateTime, out Archive aDayData);
    }
}