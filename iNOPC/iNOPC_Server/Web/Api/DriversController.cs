using iNOPC.Server.Models;
using iNOPC.Server.Models.Configurations;
using iNOPC.Server.Web.RequestTypes;
using System.IO;
using System.Linq;

namespace iNOPC.Server.Web.Api
{
	public class DriversController : Controller
	{
		public object Create(DriverForm data)
		{
			if (AccessedType != AccessType.FULL && AccessedType != AccessType.WRITE)
			{
				return new { Warning = "Нет доступа" };
			}

			// Проверка формы
			if (string.IsNullOrEmpty(data.Name)) return new { Error = "Не указано наименование драйвера" };
			if (string.IsNullOrEmpty(data.Path)) return new { Error = "Не указан путь к DLL сборке драйвера" };

			lock (Program.Configuration.Drivers)
			{
				if (Program.Configuration.Drivers.Count(x => x.Name == data.Name) > 0)
				{
					return new { Error = "Уже существует драйвер с таким именем" };
				}
				else
				{
					// Добавление нового драйвера
					var driver = new Driver
					{
						Id = Program.Configuration.NextId++,
						Name = data.Name,
						Path = Program.Base + @"\Drivers\" + data.Path + ".dll",
					};

					Program.Configuration.Drivers.Add(driver);
					Program.Configuration.SaveToFile();

					driver.Load();

					WebSocket.Broadcast("tree");
					WebSocket.Broadcast("driver:" + Program.Configuration.NextId);

					return new { Program.Configuration.NextId };
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
				return Program.Configuration.Drivers
					.Select(x => new
					{
						x.Id,
						x.Name,
						Path = x.Path.Replace(Program.Base + @"\Drivers\", "").Replace(".dll", ""),
						Dlls = CreateForm()
					})
					.FirstOrDefault(x => x.Id == data.Id);
			}
		}

		public object Update(DriverForm data)
		{
			if (AccessedType != AccessType.FULL && AccessedType != AccessType.WRITE)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				var driver = Program.Configuration.Drivers.FirstOrDefault(x => x.Id == data.Id);

				if (driver != null)
				{
					driver.Name = data.Name;
					driver.Path = Program.Base + @"\Drivers\" + data.Path + ".dll";

					foreach (var device in driver.Devices)
					{
						device.DriverName = data.Name;
					}

					Program.Configuration.SaveToFile();
					driver.Load();

					WebSocket.Broadcast("tree");
					WebSocket.Broadcast("driver:" + driver.Id);

					return true;
				}
			}

			return false;
		}

		public object Delete(IdOnly data)
		{
			if (AccessedType != AccessType.FULL && AccessedType != AccessType.WRITE)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				var driver = Program.Configuration.Drivers.FirstOrDefault(x => x.Id == data.Id);

				if (driver != null)
				{
					foreach (var device in driver.Devices) device.Stop();
					Program.Configuration.Drivers.Remove(driver);
					Program.Configuration.SaveToFile();

					WebSocket.Broadcast("tree");
					WebSocket.Broadcast("driver:" + driver.Id);

					return true;
				}
			}

			return false;
		}

		public object Reload(IdOnly data)
		{
			if (AccessedType != AccessType.FULL && AccessedType != AccessType.WRITE)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				var driver = Program.Configuration.Drivers.FirstOrDefault(x => x.Id == data.Id);

				if (driver != null)
				{
					var reload = driver.Load();

					WebSocket.Broadcast("tree");
					WebSocket.Broadcast("driver:" + driver.Id);
					return reload;
				}
			}

			return false;
		}
		
		public object Devices(IdOnly data)
		{
			if (AccessedType != AccessType.READ && AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				return Program.Configuration.Drivers
					.Where(x => x.Id == data.Id)
					.Select(x => x.Devices
						.Select(y => new
						{
							y.Id,
							y.Name,
							IsActive = y.Active
						})
					 )
					.FirstOrDefault();
			}
		}

		public object Logs(IdOnly data)
		{
			if (AccessedType != AccessType.READ && AccessedType != AccessType.WRITE && AccessedType != AccessType.FULL)
			{
				return new { Warning = "Нет доступа" };
			}

			lock (Program.Configuration.Drivers)
			{
				var driver = Program.Configuration.Drivers.FirstOrDefault(x => x.Id == data.Id);
				if (driver != null)
				{
					return driver.Logs.Select(x => new
					{
						Date = x.Date.ToString("dd.MM.yyyy HH:mm:ss"),
						x.Type,
						x.Text,
					});
				}
				else
				{
					return new string[0];
				}
			}
		}

		public object CreateForm()
		{
			if (AccessedType != AccessType.FULL && AccessedType != AccessType.WRITE)
			{
				return new { Warning = "Нет доступа" };
			}

			var excluded = new[] { "lib60870", "Newtonsoft.Json", "iNOPC_Library", "iNOPC_Server" };

			return Directory
				.GetFiles(Program.Base + @"\Drivers", "*.dll")
				.Select(x => x.Replace(Program.Base + @"\Drivers\", "").Replace(".dll", ""))
				.Where(x => !excluded.Contains(x))
				.ToArray();
		}
	}
}
