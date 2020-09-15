using iNOPC.Server.Models;
using iNOPC.Server.Web;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace iNOPC.Server
{
    class Program
    {
        public static string Base { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        static void Main()
        {
            // Проверка на наличие ранее запущенного экземпляра сервера
            bool alreadyWorking = Process.GetProcesses().Where(x => x.ProcessName.Contains("iNOPC")).Count() > 1;

            if (alreadyWorking)
            {
                Err("Уже существует другой запущенный экземпляр сервера.");
                Console.WriteLine("Уже существует другой запущенный экземпляр сервера. Нажмите Enter для выхода.");
                Console.ReadLine();
                return;
            }

            if (Environment.UserInteractive)
            { 
                Start();

                Console.WriteLine("Сервер запущен. Нажмите Enter для выхода.");
                Console.ReadLine();

                Stop();
            }
            else
            {
                // Запуск в качестве службы. Старт и стоп в службе автоматизированы.
                // После старта программа останется в работе, даже если все действия будут выполнены, и завершится по команде стоп
                using (var service = new Service())
                {
                    ServiceBase.Run(service);
                }
            }
        }

        public static void Start()
        {
            // Стартуем необходимые сервисы
            Task.Run(Http.Start);
            WebSocket.Start();
            OPC.StartServer();

            // Загружаем конфиг
            Storage.Load();
            OPC.RefreshAllClients();
        }

        public static void Stop()
        {
            // Сохраняем конфигурацию
            Storage.Save();
            foreach (var driver in Storage.Drivers)
            {
                foreach (var device in driver.Devices.Where(x => x.Active))
                {
                    device.Stop();
                }
            }

            // Глушим OPC сервер
            OPC.RequestDisconnect();
            OPC.UninitWTOPCsvr();

            // Глушим вебку
            WebSocket.Stop();
            Http.Stop();
        }

        public static void Err(string text = "Runtime error")
        {
            EventLog.WriteEntry("iNOPC_Server", text);
        }
    }

    public class Service : ServiceBase
    {
        public Service()
        {
            ServiceName = "iNOPC_Server";
        }

        protected override void OnStart(string[] args)
        {
            Program.Start();
        }

        protected override void OnStop()
        {
            Program.Stop();
        }
    }
}