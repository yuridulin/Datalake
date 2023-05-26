using LinqToDB;
using Logger.Database;
using Logger.Library;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Logger.Workers
{
	public static class DeployWorker
	{
		public static async Task Work(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					using (var db = new DatabaseContext())
					{
						var endpointsToInstall = await db.Stations
							.Where(x => x.DeployStatus == StationDeployState.WaitForInstall)
							.Select(x => x.Endpoint)
							.ToListAsync();

						var endpointsToRemove = await db.Stations
							.Where(x => x.DeployStatus == StationDeployState.WaitForUninstall)
							.Select(x => x.Endpoint)
							.ToListAsync();

						foreach (var endpoint in endpointsToInstall)
						{
							await db.Stations
								.Where(x => x.Endpoint == endpoint)
								.Set(x => x.DeployStatus, StationDeployState.Installing)
								.Set(x => x.DeployMessage, string.Empty)
								.Set(x => x.DeployTime, DateTime.Now)
								.UpdateAsync();

							string err = Install(endpoint);

							await db.Stations
								.Where(x => x.Endpoint == endpoint)
								.Set(x => x.DeployStatus, string.IsNullOrEmpty(err) ? StationDeployState.Installed : StationDeployState.ErrorWhenInstall)
								.Set(x => x.DeployMessage, err)
								.Set(x => x.DeployTime, DateTime.Now)
								.UpdateAsync();

							Helpers.RaiseServerEvent("DeployWorker", "Install: " + endpoint);
						}

						foreach (var endpoint in endpointsToRemove)
						{
							await db.Stations
								.Where(x => x.Endpoint == endpoint)
								.Set(x => x.DeployStatus, StationDeployState.WaitForUninstall)
								.Set(x => x.DeployMessage, string.Empty)
								.Set(x => x.DeployTime, DateTime.Now)
								.UpdateAsync();

							string err = Remove(endpoint);

							await db.Stations
								.Where(x => x.Endpoint == endpoint)
								.Set(x => x.DeployStatus, string.IsNullOrEmpty(err) ? StationDeployState.Uninstalled : StationDeployState.ErrorWhenUninstall)
								.Set(x => x.DeployMessage, err)
								.Set(x => x.DeployTime, DateTime.Now)
								.UpdateAsync();

							Helpers.RaiseServerEvent("DeployWorker", "Uninstall: " + endpoint);
						}
					}
				}
				catch (Exception e)
				{
					Helpers.RaiseServerEvent("DeployWorker", "Error: " + e.Message);
				}

				// ожидание следующего подхода
				Task.Delay(TimeSpan.FromSeconds(5)).Wait();
			}
		}

		static string Install(string endpoint)
		{
			try
			{
				// копирование файлов агента на удаленную машину
				string source = AppDomain.CurrentDomain.BaseDirectory + "\\Repository\\Agent\\";
				string dest = "\\\\" + endpoint + "\\c$\\Program Files\\Logger\\Logger Agent\\";
				if (Directory.Exists(dest)) { Directory.Delete(dest, true); }
				Copy(source, dest);

				// определение конфигурации для агента
				string localIP;
				using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
				{
					socket.Connect(endpoint, 4330);
					IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
					localIP = endPoint.Address.ToString();
				}

				var config = new AgentGlobalConfig
				{
					Server = localIP,
					Port = 4330,
					ReplyIntervalSeconds = 10,
				};
				string configFile = dest + "config.json";
				File.WriteAllText(configFile, JsonConvert.SerializeObject(config));

				// создание службы
				try { Cmd("/c sc \\\\" + endpoint + " create \"LoggerAgent\" DisplayName=\"Logger Agent\" binPath=\"C:\\Program Files\\Logger\\Logger Agent\\Logger Agent.exe\" start=auto"); } catch { }

				// запуск службы
				try { Cmd("/c sc \\\\" + endpoint + " start LoggerAgent"); } catch { }

				return string.Empty;
			}
			catch (Exception e)
			{
				Helpers.RaiseServerEvent("DeployWorker", "Install: " + e.Message);
				return e.Message;
			}
		}

		static string Remove(string endpoint)
		{
			try
			{
				// остановка службы
				Cmd("/c sc \\\\" + endpoint + " stop LoggerAgent");

				// Удаление записи в реестре
				// Нужно, потому что при удалении из оснастки служба всего лишь помечается на удаление после перезагрузки
				try
				{
					using (var view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
					{
						using (var service = view.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true))
						{
							service.DeleteSubKeyTree("LoggerAgent");
						}
					}
				}
				catch { }

				// Удаление записи в оснастке
				Cmd("/c sc \\\\" + endpoint + " delete LoggerAgent");

				// Удаление файлов
				string dest = "\\\\" + endpoint + "\\c$\\Program Files\\Logger\\Logger Agent";
				if (Directory.Exists(dest))
				{
					Directory.Delete(dest, true);
				}
				else
				{
					Helpers.RaiseServerEvent("DeployWorker", "Delete not-exist directory: " + dest);
				}

				return string.Empty;
			}
			catch (Exception e)
			{
				Helpers.RaiseServerEvent("DeployWorker", "Uninstall error:\r\n" + e.Message + "\r\n" + e.StackTrace);
				return e.Message;
			}
		}

		static void Cmd(string command)
		{
			using (var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					UseShellExecute = false,
					FileName = "cmd.exe",
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					Arguments = command
				}
			})
			{
				process.Start();
				process.WaitForExit();
			}
		}

		static void Copy(string source, string destination)
		{
			if (!Directory.Exists(destination)) Directory.CreateDirectory(destination);

			foreach (var file in Directory.GetFiles(source))
			{
				File.Copy(file, file.Replace(source, destination), true);
			}

			foreach (var sourceSubfolder in Directory.EnumerateDirectories(source))
			{
				var destinationSubfolder = sourceSubfolder.Replace(source, destination);
				
				Copy(sourceSubfolder, destinationSubfolder);
			}
		}
	}
}
