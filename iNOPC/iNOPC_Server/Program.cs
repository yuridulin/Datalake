using iNOPC.Server.Models;
using iNOPC.Server.Storage;
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
		public static string Base => AppDomain.CurrentDomain.BaseDirectory;

		public static string ExeName => AppDomain.CurrentDomain.FriendlyName;

		public static Configuration Configuration { get; set; } = new Configuration();

		static void Main()
		{
			// Проверка на наличие ранее запущенного экземпляра сервера
			bool alreadyWorking = Process.GetProcesses().Count(x => x.ProcessName == ExeName) > 1;

			if (alreadyWorking)
			{
				Log("Уже существует другой запущенный экземпляр сервера.\nСервер не будет запущен.\nНажмите Enter для выхода.");
				Console.ReadLine();
				return;
			}

			if (Environment.UserInteractive)
			{
				Start();
				Log("Сервер запущен\nАдрес веб-консоли для доступа к серверу: http://localhost:" + Configuration.Settings.WebConsolePort + "\nНажмите Enter для выхода.");
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
			Task.Run(AssemblyLoader.Load);
			Configuration.RestoreFromFile();
			Task.Run(Http.Start);
			OPC.StartServer();
#if !DEBUG
			Defence.Set();
#endif
			Calculator.Start();

			// Загружаем конфиг
			Configuration.Start();
			OPC.RefreshAllClients();
		}

		public static void Stop()
		{
			SweetStop();

			// Глушим вебку
			Http.Stop();

			Log("Сервер остановлен");
		}

		public static void SweetStop()
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
		}

		public static void Log(string text = "Runtime error")
		{
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