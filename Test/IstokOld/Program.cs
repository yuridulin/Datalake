using System;
using Test.Models;

namespace Test
{
	class Program
    {
        static void Main()
        {
            // Настройка COM порта

            var com = new ComPort
            {
                Number = 5,
                BaudRate = 19200,
                StopBits = 1,
                Parity = 0,
                Timeout = 1500
            };

            Istok.SetComPortConfig(com, true);


            // Подключение

            Console.WriteLine("Init COM: " + Istok.InitComPort());
            Console.WriteLine("GetComFuncError: " + Istok.GetComFuncError());


            // Получение времени прибора

            Istok.ReadDateTime(1, out DateTime time);
            Console.WriteLine("Time = " + time);


            // Чтение реальных данных

            Istok.ReadOperativeData(1, out Operative data);
            Console.WriteLine("Cold Source:" + 
                "\n\th = " + data.ColdSourceData.h + 
                "\n\tP = " + data.ColdSourceData.P +
                "\n\tt = " + data.ColdSourceData.t + 
                "\n\tPatm = " + data.ColdSourceData.Patm);

            for (int i = 0; i < data.PointData.Length; i++)
            {
                Console.WriteLine("Point " + i + ":" +
                    "\n\tG = " + data.PointData[i].G +
                    "\n\th = " + data.PointData[i].h +
                    "\n\tP = " + data.PointData[i].P +
                    "\n\tQ = " + data.PointData[i].Q +
                    "\n\tr = " + data.PointData[i].r +
                    "\n\tt = " + data.PointData[i].t +
                    "\n\tV = " + data.PointData[i].V + 
                    "\n\tdP = " + data.PointData[i].dP);
            }


            // Отключение

            Console.WriteLine("Free COM: " + Istok.FreeComPort());
            Console.ReadLine();
        }
    }
}