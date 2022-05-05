using System;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Server.Models
{
    public static class Calculator
    {
        public static void Start()
        {
            Timer = new Timer(1000);
            Timer.Elapsed += (s, e) => Calculate();

            Prepare();

            Timer.Start();
        }

        public static void Stop()
        {
            Timer.Stop();
        }

        public static void Reset()
        {
            Prepare();
            Task.Run(Calculate);
        }


        static Timer Timer { get; set; }

        static void Prepare()
        {
            lock (Program.Configuration.Formulars)
            {
                foreach (var f in Program.Configuration.Formulars)
                {
                    f.Set();
                }
            }
        }

        static void Calculate()
        {
            lock (Program.Configuration.Formulars)
            {
                DateTime date = DateTime.Now;

                foreach (var f in Program.Configuration.Formulars)
                {
                    f.Calculate(date);
                }
            }
        }
    }
}