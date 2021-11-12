using iNOPC.Library;
using iNOPC.Server.Models;
using iNOPC.Server.Web.RequestTypes;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace iNOPC.Server.Web
{
	public class Requester
    {
        AccessType AccessType { get; set; } = AccessType.GUEST;

        HttpListenerRequest Request { get; set; }

        HttpListenerResponse Response { get; set; }


        public void CreateResponse(HttpListenerContext context)
        {
            Request = context.Request;
            Response = context.Response;

            // Определение запрашиваемого действия
            string requestBody = "";
            if (Request.InputStream != Stream.Null)
            {
                using (var reader = new StreamReader(Request.InputStream, Request.ContentEncoding))
                {
                    requestBody = reader.ReadToEnd();
                }
            }

            // Подготовка ответа
            var url = Request.Url.LocalPath.Substring(1).Replace('/', '\\');
            if (string.IsNullOrEmpty(url))
            {
                url = "index.html";
            }

            string responseString;
            try
            {
                if (File.Exists(Http.Base + url))
                {
                    responseString = File.ReadAllText(Http.Base + url);

                    var type = url.Substring(url.LastIndexOf('.') + 1);
                    switch (type)
                    {
                        case "css":
                            Response.ContentType = "text/css";
                            break;
                        case "png":
                            Response.ContentType = "image/png";
                            break;
                        case "html":
                            Response.ContentType = "text/html";
                            break;
                        case "js":
                            Response.ContentType = "application/javascript";
                            break;
                        case "ico":
                            Response.ContentType = "image/x-icon";
                            break;
                    }
                }
                else
                {
                    // авторизация запроса по токену, хранящемуся в куках. Если запрос - аутентификация, то обработчик перезапишет это значение
                    if (url != "login")
                    {
                        // Так как токен перезаписывается при каждом запросе, сразу устанавливаем header с пришедшим
                        // Запросы login и logout обрабатываются отдельно
                        var token = Request.Headers.Get("Inopc-Access-Token");
                        Response.Headers.Add("Inopc-Access-Token", token);

                        var session = Http.Sessions.FirstOrDefault(x => token != null && x.Token == token && x.Expire > DateTime.Now);
                        if (session != null)
                        {
                            // Пользователь авторизован по сессионному токену
                            AccessType = session.AccessType;
                            Response.Headers.Add("Inopc-Login", session.Login);
                        }
                        else if (Program.Configuration.Access.Count != 0)
                        {
                            // Пользователь не авторизован, выдаётся гостевой доступ
                            AccessType = AccessType.GUEST;
                            Response.Headers.Add("Inopc-Login", "guest");
                        }
                        else
                        {
                            // Ни одна учётная запись не создана, выдается специальное разрешение FIRST для создания первой учётной записи
                            AccessType = AccessType.FIRST;
                            Response.Headers.Add("Inopc-Login", "first");
                        }

                        Response.Headers.Add("Inopc-Access-Type", ((int)AccessType).ToString());
                    }

                    Response.ContentType = "application/json";
                    responseString = JsonConvert.SerializeObject(Action(url, requestBody));
                }
            }
            catch (Exception e)
            {
                Response.ContentType = "application/json";
                responseString = JsonConvert.SerializeObject(new { Error = e.Message, e.StackTrace });
            }


            // Отправка ответа клиенту

            var buffer = Encoding.UTF8.GetBytes(responseString);
            Response.StatusCode = 200;
            Response.ContentLength64 = buffer.Length;
            using (var output = Response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }

        object Action(string methodName, string body)
        {
            try
            {
                switch (methodName)
                {
                    case "login": return Login(body);
                    case "logout": return Logout(body);
                    case "users": return Users();
                    case "user.create": return UserCreate(body);
                    case "user.delete": return UserDelete(body);

                    case "tree": return Tree();

                    case "settings": return Settings();
                    case "opc.install": return OpcInstallDcom();
                    case "opc.uninstall": return OpcUninstallDcom();
                    case "opc.clean": return OpcClean();
                    case "service.create": return ServiceCreate();
                    case "service.remove": return ServiceRemove();

                    case "driver": return Driver(body);
                    case "driver.devices": return DriverDevices(body);
                    case "driver.logs": return DriverLogs(body);
                    case "driver.createform": return DriverCreateForm();
                    case "driver.create": return DriverCreate(body);
                    case "driver.update": return DriverUpdate(body);
                    case "driver.delete": return DriverDelete(body);
                    case "driver.reload": return DriverReload(body);

                    case "device": return Device(body);
                    case "device.create": return DeviceCreate(body);
                    case "device.update": return DeviceUpdate(body);
                    case "device.delete": return DeviceDelete(body);
                    case "device.fields": return DeviceFields(body);
                    case "device.logs": return DeviceLogs(body);
                    case "device.configuration": return DeviceConfiguration(body);
                    case "device.start": return DeviceStart(body);
                    case "device.stop": return DeviceStop(body);

                    default: return new { Error = "Запрошенный метод не существует", StackTrace = "Web.Controller.Action" };
                }
            }
            catch (Exception e)
            {
                return new { Error = e.Message, e.StackTrace };
            }
        }

        object Login(string body)
        {
            var user = JsonConvert.DeserializeObject<LoginPass>(body);

            var auth = Program.Configuration.Access
                .Where(x => x.Login == user.Login)
                .FirstOrDefault();

            if (auth == null)
            {
                return new { Warning = "Указанная учетная запись не найдена" };
            }
            else if (auth.Hash != user.Hash)
            {
                return new { Warning = "Введенный пароль не подходит" };
            }
            else
            {
                var session = new Session
                {
                    Login = auth.Login,
                    Token = new Random().Next().ToString(),
                    AccessType = auth.AccessType,
                    Expire = DateTime.Now.AddDays(7),
                };

                lock (Http.Sessions)
                {
                    Http.Sessions.Add(session);
                }

                // Записываем в куки клиента его сессионный токен
                Response.Headers.Add("Inopc-Access-Token", session.Token);

                // Авторизовываем, чтобы после перезагрузки вебки клиент сразу вошел
                // честно говоря лишняя деталь на случай, когда вместо полного релоада будут перегружаться конкретные компоненты вебки
                //response.Headers.Add("Inopc-Login", user.Login);
                //response.Headers.Add("Inopc-Access-Type", ((int)session.AccessType).ToString());

                return new { Done = "Вход успешно выполнен" };
            }
        }

        object Logout(string body)
        {
            var session = JsonConvert.DeserializeObject<Session>(body);

            lock (Http.Sessions)
            {
                var old = Http.Sessions.Where(x => x.Token == session.Token).FirstOrDefault();
                if (old != null)
                {
                    Http.Sessions.Remove(old);
                }
            }

            return new { Done = "Выход произведён успешно" };
        }

        object Users()
        {
            if (AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            lock (Program.Configuration)
            {
                return Program.Configuration.Access
                    .Select(x => new
                    {
                        x.Login,
                        x.AccessType,
                    });
            }
        }

        object UserCreate(string body)
        {
            if (AccessType != AccessType.FULL && AccessType != AccessType.FIRST)
            {
                return new { Warning = "Нет доступа" };
            }

            var user = JsonConvert.DeserializeObject<LoginPass>(body);
            var access = new AccessRecord
            {
                Login = user.Login,
                AccessType = user.AccessType,
                Hash = user.Hash,
            };

            lock (Program.Configuration)
            {
                var another = Program.Configuration.Access
                    .FirstOrDefault(x => x.Login == access.Login);

                if (another != null) return new { Warning = "Такой пользователь уже существует" };

                Program.Configuration.Access.Add(access);
                Program.Configuration.SaveToFile();
            }

            return new { Done = "Пользователь успешно добавлен" };
        }

        object UserDelete(string body)
        {
            if (AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var user = JsonConvert.DeserializeObject<LoginPass>(body);

            lock (Program.Configuration)
            {
                var access = Program.Configuration.Access
                    .FirstOrDefault(x => x.Login == user.Login);

                if (access == null) return new { Warning = "Такой пользователь не существует" };

                Program.Configuration.Access.Remove(access);
                Program.Configuration.SaveToFile();
            }

            return new { Done = "Пользователь успешно удалён" };
        }


        object Tree()
        {
            if (AccessType != AccessType.READ && AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
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


        object Settings()
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
                OpcStatus = foundInDCOM ? "сервер зарегистрирован" : "сервер не зарегистрирован",
                ServiceStatus = path == "" 
                    ? "не установлена"
                    : path == Program.Base + Program.ExeName
                        ? "установлена для этого сервера"
                        : "установлена для другого сервера",
                InitPath = Program.Base + Program.ExeName,
                ServicePath = path == ""
                    ? "не задан"
                    : path,
            };
        }

        object OpcInstallDcom()
        {
            if (AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            OPC.InstallDCOM();

            return new { Done = true };
        }

        object OpcUninstallDcom()
		{
            if (AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            OPC.UninstallDCOM();

            return new { Done = true };
        }

        object OpcClean()
        {
            if (AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            OPC.CleanOldTags();

            return new { Done = true };
        }

        object ServiceCreate()
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
            return "Служба успешно создана";
        }

        object ServiceRemove()
        {
            // Удаление записи в реестре
            // Нужно, потому что при удалении из оснастки служба всего лишь помечается на удаление после перезагрузки
            using (var view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
            {
                using (var service = view.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true))
                {
                    service.DeleteSubKeyTree("iNOPC");
                }
            }

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
            return "Служба успешно удалена";
        }


        object Driver(string body)
        {
            if (AccessType != AccessType.READ && AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

            lock (Program.Configuration.Drivers)
            {
                return Program.Configuration.Drivers
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        Path = x.Path.Replace(Program.Base + @"\Drivers\", "").Replace(".dll", ""),
                        Dlls = DriverCreateForm()
                    })
                    .FirstOrDefault(x => x.Id == data.Id);
            }
        }

        object DriverDevices(string body)
        {
            if (AccessType != AccessType.READ && AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

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

        object DriverLogs(string body)
        {
            if (AccessType != AccessType.READ && AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

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

        object DriverCreateForm()
        {
            if (AccessType != AccessType.FULL && AccessType != AccessType.WRITE)
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

        object DriverCreate(string body)
        {
            if (AccessType != AccessType.FULL && AccessType != AccessType.WRITE)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<DriverForm>(body);

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
                        Id = ++Program.Configuration.NextId,
                        Name = data.Name,
                        Path = Program.Base + @"\Drivers\" + data.Path + ".dll",
                    };

                    Program.Configuration.Drivers.Add(driver);
                    Program.Configuration.SaveToFile();

                    driver.Load();

                    WebSocket.Broadcast("tree");
                    WebSocket.Broadcast("driver:" + driver.Id);

                    return new { driver.Id };
                }
            }
        }

        object DriverUpdate(string body)
        {
            if (AccessType != AccessType.FULL && AccessType != AccessType.WRITE)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<DriverForm>(body);

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

        object DriverDelete(string body)
        {
            if (AccessType != AccessType.FULL && AccessType != AccessType.WRITE)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

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

        object DriverReload(string body)
        {
            if (AccessType != AccessType.FULL && AccessType != AccessType.WRITE)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

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

        object Device(string body)
        {
            if (AccessType != AccessType.READ && AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

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

        object DeviceFields(string body)
        {
            if (AccessType != AccessType.READ && AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

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

        object DeviceLogs(string body)
        {
            if (AccessType != AccessType.READ && AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

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

        object DeviceConfiguration(string body)
        {
            if (AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

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

        object DeviceCreate(string body)
        {
            if (AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

            lock (Program.Configuration.Drivers)
            {
                var driver = Program.Configuration.Drivers.FirstOrDefault(x => x.Id == data.Id);

                if (driver != null)
                {
                    var device = new Device
                    {
                        Id = ++Program.Configuration.NextId,
                        Name = "New_Device_" + Program.Configuration.NextId,
                        AutoStart = false,
                        Configuration = driver.DefaultConfiguratuon,
                        DriverId = driver.Id,
                        DriverName = driver.Name,
                    };

                    driver.Devices.Add(device);

                    Program.Configuration.SaveToFile();

                    WebSocket.Broadcast("tree");
                    WebSocket.Broadcast("driver.devices:" + driver.Id);

                    return new { device.Id };
                }
                else
                {
                    return new { Error = "Драйвер не найден. Попробуйте обновить страницу" };
                }
            }
        }

        object DeviceUpdate(string body)
        {
            if (AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<DeviceForm>(body);

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

                        WebSocket.Broadcast("tree");
                        WebSocket.Broadcast("driver.devices:" + driver.Id);

                        return true;
                    }
                }
            }

            return null;
        }

        object DeviceDelete(string body)
        {
            if (AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

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

                        WebSocket.Broadcast("tree");
                        WebSocket.Broadcast("driver.devices:" + driver.Id);

                        return true;
                    }
                }
            }

            return null;
        }

        object DeviceStart(string body)
        {
            if (AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

            lock (Program.Configuration.Drivers)
            {
                foreach (var driver in Program.Configuration.Drivers)
                {
                    var device = driver.Devices.FirstOrDefault(x => x.Id == data.Id);

                    if (device != null)
                    {
                        device.Start();

                        WebSocket.Broadcast("tree");
                        WebSocket.Broadcast("driver.devices:" + driver.Id);

                        return true;
                    }
                }
            }

            return null;
        }

        object DeviceStop(string body)
        {
            if (AccessType != AccessType.WRITE && AccessType != AccessType.FULL)
            {
                return new { Warning = "Нет доступа" };
            }

            var data = JsonConvert.DeserializeObject<IdOnly>(body);

            lock (Program.Configuration.Drivers)
            {
                foreach (var driver in Program.Configuration.Drivers)
                {
                    var device = driver.Devices.FirstOrDefault(x => x.Id == data.Id);

                    if (device != null)
                    {
                        device.Stop();

                        WebSocket.Broadcast("tree");
                        WebSocket.Broadcast("driver.devices:" + driver.Id);

                        return true;
                    }
                }
            }

            return null;
        }
    }
}