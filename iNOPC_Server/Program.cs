using iNOPC.Server.Models;
using iNOPC.Server.Web;
using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace iNOPC.Server
{
	class Program
    {
        public static string Base { get; set; } = AppDomain.CurrentDomain.BaseDirectory;

        public static Configuration Configuration { get; set; } = new Configuration();

        static void Main()
        {
            // Проверка на наличие ранее запущенного экземпляра сервера
            bool alreadyWorking = Process.GetProcesses().Where(x => x.ProcessName.Contains("iNOPC")).Count() > 1;

            if (alreadyWorking)
            {
                Log("Уже существует другой запущенный экземпляр сервера.");
                Console.ReadLine();
                return;
            }

            if (Environment.UserInteractive)
            {
#if DEBUG
                Start();
#else
                // Запуск в качестве exe регистрирует службу, а потом идёт на отдых
                Log("Производится настройка...");
                OPC.InitDCOM();
                Log("Настройка DCOM выполнена. Создана служба iNOPC. После запуска службы сервер готов к работе.");
                Log("Адрес веб-консоли для доступа к серверу: http://localhost:81");
                Log("Нажмите Enter для выхода.");
#endif

				Console.ReadLine();
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
            //WebSocket.Start();
            OPC.StartServer();

            // Загружаем конфиг
            Configuration.RestoreFromFile();
            OPC.RefreshAllClients();
        }

        public static void Stop()
        {
            // Сохраняем конфигурацию
            Configuration.SaveToFile();

            // Останавливаем запущенные модули
            foreach (var driver in Configuration.Drivers)
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
            //WebSocket.Stop();
            Http.Stop();

            Log("Сервер остановлен");
        }

        public static void Log(string text = "Runtime error")
        {
            EventLog.WriteEntry("iNOPC_Server", text);
            Console.WriteLine(DateTime.Now + " > " + text);
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