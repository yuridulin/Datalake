using iNOPC.Library;
using iNOPC.Server.Models;
using iNOPC.Server.Web.RequestTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace iNOPC.Server.Web
{
    public class Http
    {
        public static List<Session> Sessions = new List<Session>();

        public static async Task Start()
        {
            Listener = new HttpListener();
            Listener.Prefixes.Add("http://*:81/");
            Listener.Start();

            while (true)
            {
                Process(await Listener.GetContextAsync());
            }
        }

        public static void Stop()
        {
            Listener.Stop();
        }


        static HttpListener Listener { get; set; }

        static string Base { get; set; } = Program.Base + "\\webConsole\\";

        static void Process(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            // Определение запрашиваемого действия
            string requestBody = "";
            if (request.InputStream != Stream.Null)
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    requestBody = reader.ReadToEnd();
                }
            }

            // Подготовка ответа
            var url = request.Url.LocalPath.Substring(1).Replace('/', '\\');
            if (string.IsNullOrEmpty(url))
            {
                url = "index.html";
            }

            string responseString;
            try
            {
                if (File.Exists(Base + url))
                {
                    responseString = File.ReadAllText(Base + url);

                    var type = url.Substring(url.LastIndexOf('.') + 1);
                    switch (type)
                    {
                        case "css":
                            response.ContentType = "text/css";
                            break;
                        case "png":
                            response.ContentType = "image/png";
                            break;
                        case "html":
                            response.ContentType = "text/html";
                            break;
                        case "js":
                            response.ContentType = "application/javascript";
                            break;
                        case "ico":
                            response.ContentType = "image/x-icon";
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
                        var token = request.Headers.Get("Inopc-Access-Token");
                        response.Headers.Add("Inopc-Access-Token", token);

                        var session = Sessions.FirstOrDefault(x => token != null && x.Token == token && x.Expire > DateTime.Now);
                        if (session != null)
						{
                            // Пользователь авторизован по сессионному токену
                            response.Headers.Add("Inopc-Access-Type", ((int)session.AccessType).ToString());
                            response.Headers.Add("Inopc-Login", session.Login);

                            //Console.WriteLine(url + " > " + session.Token + " = " + session.Login + "|" + session.AccessType);
                        }
                        else if (Program.Configuration.Access.Count != 0)
						{
                            // Пользователь не авторизован, выдаётся гостевой доступ
                            response.Headers.Add("Inopc-Access-Type", ((int)AccessType.GUEST).ToString());
                            response.Headers.Add("Inopc-Login", "guest");

                            //Console.WriteLine(url + " > no token = guest|GUEST");
                        }
                        else
						{
                            // Ни одна учётная запись не создана, выдается специальное разрешение FIRST для создания первой учётной записи
                            response.Headers.Add("Inopc-Access-Type", ((int)AccessType.FIRST).ToString());
                            response.Headers.Add("Inopc-Login", "first");

                            //Console.WriteLine(url + " > no token = first|FULL");
                        }
                    }

                    response.ContentType = "application/json";
                    responseString = JsonConvert.SerializeObject(Action(url, requestBody, ref response));
                }
            }
            catch (Exception e)
            {
                response.ContentType = "application/json";
                responseString = JsonConvert.SerializeObject(new { Error = e.Message, e.StackTrace });
            }


            // Отправка ответа клиенту
            
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.StatusCode = 200;
            response.ContentLength64 = buffer.Length;
            using (var output = response.OutputStream)
            {
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
        }

        static object Action(string methodName, string body, ref HttpListenerResponse resp)
        {
            try
            {
                switch (methodName)
                {
                    case "login": return Login(body, ref resp);
                    case "logout": return Logout(body, ref resp);
                    case "users": return Users();
                    case "user.create": return UserCreate(body);
                    case "user.delete": return UserDelete(body);

                    case "tree": return Tree();

                    case "opc.dcom": return OpcDcom();
                    case "opc.clean": return OpcClean();

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


        static object Login(string body, ref HttpListenerResponse response)
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

                Sessions.Add(session);

                // Записываем в куки клиента его сессионный токен
                response.Headers.Add("Inopc-Access-Token", session.Token);

                // Авторизовываем, чтобы после перезагрузки вебки клиент сразу вошел
                // честно говоря лишняя деталь на случай, когда вместо полного релоада будут перегружаться конкретные компоненты вебки
                //response.Headers.Add("Inopc-Login", user.Login);
                //response.Headers.Add("Inopc-Access-Type", ((int)session.AccessType).ToString());

                return new { Done = "Вход успешно выполнен" };
            }
		}

        static object Logout(string body, ref HttpListenerResponse response)
		{
            var session = JsonConvert.DeserializeObject<Session>(body);
            var old = Sessions.Where(x => x.Token == session.Token).FirstOrDefault();

            if (old != null)
			{
                Sessions.Remove(old);
			}

            return new { Done = "Выход произведён успешно" };
		}

        static object Users()
		{
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

        static object UserCreate(string body)
		{
            try
            {
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
            catch (Exception e)
			{
                return new { Error = e.Message };
			}
		}

        static object UserDelete(string body)
        {
            try
            {
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
            catch (Exception e)
            {
                return new { Error = e.Message };
            }
        }


        static object Tree()
        {
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

        static object OpcDcom()
        {
            OPC.InitDCOM();
            return true;
        }

        static object OpcClean()
        {
            OPC.CleanOldTags();
            return true;
        }

        static object Driver(string body)
        {
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

        static object DriverDevices(string body)
        {
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

        static object DriverLogs(string body)
        {
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

        static object DriverCreateForm()
        {
            var excluded = new[] { "lib60870", "Newtonsoft.Json", "iNOPC_Library", "iNOPC_Server" };

            try
            {
                return Directory
                    .GetFiles(Program.Base + @"\Drivers", "*.dll")
                    .Select(x => x.Replace(Program.Base + @"\Drivers\", "").Replace(".dll", ""))
                    .Where(x => !excluded.Contains(x))
                    .ToArray();
            }
            catch (Exception)
            {
                return new string[0];
            }

        }

        static object DriverCreate(string body)
        {
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

        static object DriverUpdate(string body)
        {
            var data = JsonConvert.DeserializeObject<DriverForm>(body);

            lock (Program.Configuration.Drivers)
            {
                var driver = Program.Configuration.Drivers.FirstOrDefault(x => x.Id == data.Id);

                if (driver != null)
                {
                    driver.Name = data.Name;
                    driver.Path = Program.Base + @"\Drivers\" + data.Path + ".dll";

                    Program.Configuration.SaveToFile();
                    driver.Load();

                    WebSocket.Broadcast("tree");
                    WebSocket.Broadcast("driver:" + driver.Id);

                    return true;
                }
            }

            return false;
        }

        static object DriverDelete(string body)
        {
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

        static object DriverReload(string body)
        {
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

        static object Device(string body)
        {
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

            return null;
        }

        static object DeviceFields(string body)
        {
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

        static object DeviceLogs(string body)
        {
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

        static object DeviceConfiguration(string body)
        {
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

        static object DeviceCreate(string body)
        {
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

        static object DeviceUpdate(string body)
        {
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

        static object DeviceDelete(string body)
        {
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

        static object DeviceStart(string body)
        {
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

        static object DeviceStop(string body)
        {
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