using iNOPC.Server.Models;
using iNOPC.Server.Models.Configurations;
using iNOPC.Server.Web.RequestTypes;
using Microsoft.Win32;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using iNOPC.Server.Storage;

namespace iNOPC.Server.Web.Api
{
	public class SettingsController : Controller
	{
		public object Tree()
		{
			if (AccessedType != AccessType.READ && AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			return Program.Configuration.Drivers
				.Select(driver => new
				{
					driver.Id,
					driver.Name,
					Devices = driver.Devices.Select(device => new
					{
						device.Id,
						device.Name,
						IsActive = device.Active
					}),
				});
		}

		public object LastUpdate()
		{
			return Http.LastUpdate.ToString("dd.MM.yyyy HH:mm:ss");
		}

		public object Settings()
		{
			bool foundInDCOM = false;
			string path = "";

			using (var view = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64))
			{
				using (var route = view.OpenSubKey("AppID", true))
				{
					foreach (var name in route.GetSubKeyNames())
					{
						string defValue = route.OpenSubKey(name)?.GetValue(string.Empty)?.ToString() ?? null;
						if (name == Program.ExeName || defValue == Program.ExeName || name == OPC.ServerName || defValue == OPC.ServerName)
						{
							foundInDCOM = true;
						}
					}
				}
			}

			using (var view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
			{
				using (var service = view.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\iNOPC", false))
				{
					if (service != null)
					{
						path = service.GetValue("ImagePath").ToString();
					}
				}
			}

			return new
			{
				Version = typeof(Program).Assembly.GetName().Version.ToString(),
				OpcStatus = foundInDCOM ? "сервер зарегистрирован" : "сервер не зарегистрирован",
				ServiceStatus = path == ""
					? "не установлена"
					: path == Program.Base + Program.ExeName
						? "установлена для этого сервера"
						: "установлена для другого сервера",
				InitPath = Program.Base + Program.ExeName,
				LicenseStatus = Defence.License == LicenseMode.Licensed || Defence.License == LicenseMode.Debug
					? "лицензия активна"
					: Defence.License == LicenseMode.ActiveTrial
						? "триал-период активен"
						: "триал-период завершён",
				LicenseKey = Program.Configuration.Key,
				LicenseId = Defence.UniqueHardwareId,
				ServicePath = path == ""
					? "не задан"
					: path,
			};
		}

		public object OpcInstallDcom()
		{
			if (AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			OPC.InstallDCOM();

			return new { Done = true };
		}

		public object OpcUninstallDcom()
		{
			if (AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			OPC.UninstallDCOM();

			return new { Done = true };
		}

		public object OpcClean()
		{
			if (AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			OPC.CleanOldTags();

			return new { Done = true };
		}

		public object OpcLicense(License data)
		{
			if (AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration)
			{
				Program.Configuration.Key = data.Key;
				Program.Configuration.SaveToFile();
				Task.Delay(1500).Wait();

				return new { Done = "Ключ успешно сохранён" };
			}
		}

		public object ServiceCreate()
		{
			ServiceRemove();

			using (var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					CreateNoWindow = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					UseShellExecute = false,
				},
			})
			{
				process.Start();
				process.StandardInput.WriteLine("sc create " + OPC.ServerName + " binPath= \"" + Program.Base + Program.ExeName + "\" DisplayName= \"" + OPC.ServerName + "\" start= auto && exit");

				Task.Delay(5000).Wait();

				process.Close();
			}

			Program.Log("Служба успешно создана");
			return new { Done = "Служба успешно создана" };
		}

		public object ServiceRemove()
		{
			// Удаление записи в реестре
			// Нужно, потому что при удалении из оснастки служба всего лишь помечается на удаление после перезагрузки
			try
			{
				using (var view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
				{
					using (var service = view.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true))
					{
						service.DeleteSubKeyTree("iNOPC");
					}
				}
			}
			catch { }

			// Удаление записи в оснастке
			using (var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					CreateNoWindow = true,
					RedirectStandardInput = true,
					RedirectStandardOutput = true,
					UseShellExecute = false,
				},
			})
			{
				process.Start();
				process.StandardInput.WriteLine("net stop iNOPC & sc delete iNOPC");

				Task.Delay(5000).Wait();

				process.Close();
			}

			// После этого служба должна быть удалена
			Program.Log("Служба успешно удалена");
			return new { Done = "Служба успешно удалена" };
		}
	}
}
