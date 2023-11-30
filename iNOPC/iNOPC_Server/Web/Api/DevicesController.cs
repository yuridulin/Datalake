using iNOPC.Library;
using iNOPC.Server.Models;
using iNOPC.Server.Models.Configurations;
using iNOPC.Server.Web.RequestTypes;
using System.Collections.Generic;
using System.Linq;

namespace iNOPC.Server.Web.Api
{
	public class DevicesController : Controller
	{
		public object Create(IdOnly data)
		{
			if (AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				var driver = Program.Configuration.Drivers.FirstOrDefault(x => x.Id == data.Id);

				if (driver != null)
				{
					var device = new Device
					{
						Id = Program.Configuration.NextId++,
						Name = "New_Device_" + Program.Configuration.NextId,
						AutoStart = false,
						Configuration = driver.DefaultConfiguratuon,
						DriverId = driver.Id,
						DriverName = driver.Name,
					};

					driver.Devices.Add(device);

					Program.Configuration.SaveToFile();

					Http.Update();

					return new { device.Id };
				}
				else
				{
					return new { Error = "Драйвер не найден. Попробуйте обновить страницу" };
				}
			}
		}

		public object Read(IdOnly data)
		{
			if (AccessedType != AccessType.READ && AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				foreach (var driver in Program.Configuration.Drivers)
				{
					var device = driver.Devices.FirstOrDefault(x => x.Id == data.Id);

					if (device != null)
					{
						return new
						{
							device.Name,
							device.AutoStart,
							IsActive = device.Active
						};
					}
				}
			}

			return new { Warning = "Устройство не найдено" };
		}

		public object Update(DeviceForm data)
		{
			if (AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				foreach (var driver in Program.Configuration.Drivers)
				{
					var device = driver.Devices.FirstOrDefault(x => x.Id == data.Id);
					if (device != null)
					{
						var active = device.Active;
						if (active) device.Stop();

						device.Name = data.Name;
						device.AutoStart = data.AutoStart;
						device.Configuration = data.Configuration;

						Program.Configuration.SaveToFile();

						if (active || device.AutoStart) device.Start();

						Http.Update();

						return true;
					}
				}
			}

			return null;
		}

		public object Delete(IdOnly data)
		{
			if (AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				foreach (var driver in Program.Configuration.Drivers)
				{
					var device = driver.Devices.FirstOrDefault(x => x.Id == data.Id);
					if (device != null)
					{
						device.Stop();
						driver.Devices.Remove(device);

						Program.Configuration.SaveToFile();

						Http.Update();

						return true;
					}
				}
			}

			return null;
		}
		
		public object Copy(IdOnly data)
		{
			if (AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				Driver driver = null;
				Device device = null;

				foreach (var drv in Program.Configuration.Drivers)
				{
					device = drv.Devices.FirstOrDefault(x => x.Id == data.Id);
					if (device != null)
					{
						driver = drv;
						break;
					}
				}

				if (device != null && driver != null)
				{
					var copy = new Device
					{
						Id = Program.Configuration.NextId++,
						Name = "New_Device_" + Program.Configuration.NextId,
						Active = false,
						AutoStart = false,
						Configuration = device.Configuration,
						DriverId = device.DriverId,
						DriverName = device.DriverName,
					};

					driver.Devices.Add(copy);

					Program.Configuration.SaveToFile();

					Http.Update();

					return new { copy.Id };
				}
				else
				{
					return new { Error = "Устройство для копирования не найдено. Попробуйте обновить страницу" };
				}
			}
		}

		public object Fields(IdOnly data)
		{
			if (AccessedType != AccessType.READ && AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				foreach (var driver in Program.Configuration.Drivers)
				{
					var device = driver.Devices.FirstOrDefault(x => x.Id == data.Id);
					if (device != null)
					{
						return device.Fields();
					}
				}
			}

			return null;
		}

		public object Logs(IdOnly data)
		{
			if (AccessedType != AccessType.READ && AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			foreach (var driver in Program.Configuration.Drivers)
			{
				var device = driver.Devices.FirstOrDefault(x => x.Id == data.Id);
				if (device != null)
				{
					List<Log> logs;

					lock (device.Logs)
					{
						logs = device.Logs.ToList();
					}

					return logs.Select(x => new
					{
						Date = x.Date.ToString("dd.MM.yyyy HH:mm:ss"),
						x.Text,
						x.Type
					});
				}
			}

			return null;
		}

		public object Configuration(IdOnly data)
		{
			if (AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				foreach (var driver in Program.Configuration.Drivers)
				{
					foreach (var device in driver.Devices)
					{
						if (device.Id == data.Id)
						{
							return device.GetConfigurationPage();
						}
					}
				}
			}

			return null;
		}

		public object Start(IdOnly data)
		{
			if (AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				foreach (var driver in Program.Configuration.Drivers)
				{
					var device = driver.Devices.FirstOrDefault(x => x.Id == data.Id);

					if (device != null)
					{
						device.Start();

						Http.Update();

						return true;
					}
				}
			}

			return null;
		}

		public object Stop(IdOnly data)
		{
			if (AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				foreach (var driver in Program.Configuration.Drivers)
				{
					var device = driver.Devices.FirstOrDefault(x => x.Id == data.Id);

					if (device != null)
					{
						device.Stop();

						Http.Update();

						return true;
					}
				}
			}

			return null;
		}

		public object History(History data)
		{
			if (AccessedType == AccessType.GUEST || AccessedType == AccessType.FIRST)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				foreach (var driver in Program.Configuration.Drivers)
				{
					var device = driver.Devices.FirstOrDefault(x => x.Id == data.Id);
					if (device != null)
					{
						return device.Fields();
					}
				}
			}

			return null;
		}
	}
}
