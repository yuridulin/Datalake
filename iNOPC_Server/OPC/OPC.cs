using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace iNOPC.Server
{
	public static class OPC
    {
        public const string Pass = "JVRPS53R5V64226N62H4";

        private static WriteNotificationDelegate _WriteOutDelegate;

        private static UnknownItemDelegate _UnknownItemDelegate;

        public static string ServerName => Program.Configuration?.Settings?.Name ?? "iNOPC";

        public static string ExeName => AppDomain.CurrentDomain.FriendlyName;

        public static string CLSID => Program.Configuration?.Settings?.CLSID ?? "{4537357b-3739-3334-432d-303637382d34}";

        public static Dictionary<string, uint> Tags { get; set; } = new Dictionary<string, uint>();

        public static Dictionary<string, object> DefaultFields { get; set; } = new Dictionary<string, object>();

        public static void StartServer()
        {
            InitWTOPCsvr(CLSID, 1000);
            Deactivate30MinTimer(Pass);

            _WriteOutDelegate = WriteOut;
            _UnknownItemDelegate = UnknownItem;

            EnableUnknownItemNotification(_UnknownItemDelegate);
            EnableWriteNotification(_WriteOutDelegate, false);
        }

        public static void WriteOut(uint item, ref object value, ref uint result)
        {
            // Функция записи OPC тега

            foreach (var keyValue in Tags)
            {
                if (keyValue.Value == item)
                {
                    // найдено имя тега, который будет изменяться
                    // надо найти, в какой инстанс какого драйвера надо передать сигнал
                    // искать будем по имени ДРАЙВЕР.УСТРОЙСТВО.ПОЛЕ

                    var parts = keyValue.Key.Split('.');
                    string driverName = parts[0];
                    string deviceName = parts[1];
                    string fieldName = keyValue.Key.Replace(driverName + '.' + deviceName + '.', "");

                    // поиск

                    lock (Program.Configuration)
                    {
                        var driver = Program.Configuration.Drivers.FirstOrDefault(x => x.Name == driverName);
                        if (driver == null)
                        {
                            result = 1;
                            return;
                        }

                        var device = driver.Devices.FirstOrDefault(x => x.Name == deviceName);
                        if (device == null)
                        {
                            result = 2;
                            return;
                        }

                        device.Write(fieldName, value);
                        result = 0;
                    }

                    break;
                }
            }
        }

        public static void StopServer()
        {
            RequestDisconnect();
            UninitWTOPCsvr();
        }

        public static void InitDCOM()
        {
            Console.WriteLine("Инициализация DCOM запущена");

            string pathToExe = Environment.CurrentDirectory + "\\" + ExeName;

            RequestDisconnect();
            UnregisterServer(CLSID, ServerName);
            UpdateRegistry(CLSID, ServerName, ServerName, pathToExe);
            SetVendorInfo("iNOPC RUP Vitebskenergo");
            InitWTOPCsvr(CLSID, 1000);
            Deactivate30MinTimer(Pass);

            try
            {
                Process process;

                // Пересоздание службы
                if (Program.Configuration.Settings.RegisterAsService)
                {
                    Console.WriteLine("Создание службы запущено");
                    process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "cmd.exe",
                            CreateNoWindow = true,
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                        },
                    };
                    process.Start();

                    process.StandardInput.WriteLine("sc delete VST_OPC");
                    process.StandardInput.WriteLine("sc delete iNOPC");
                    process.StandardInput.WriteLine("sc create iNOPC binPath= \"" + pathToExe + "\" DisplayName= \"" + Program.Configuration.Settings.Name + "\" start= auto && exit");

                    Task.Delay(5000).Wait();
                    process.Close(); Console.WriteLine("Создание службы завершено");
                }
                else
                {
                    Console.WriteLine("Создание службы не требуется");
                }

                // ищем AppID для продолжения регистрации
                string appID = null;
                using (var view = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                {
                    using (var clsid = view.OpenSubKey(@"Software\Classes\WOW6432Node\CLSID\", false))
                    {
                        foreach (var name in clsid.GetSubKeyNames())
                        {
                            var sub = clsid.OpenSubKey(name);
                            if (sub.GetValue(string.Empty)?.ToString() == ServerName)
                            {
                                appID = sub.GetValue("AppID").ToString();
                                break;
                            }
                        }
                    }
                }

                // добавляем записи для доступа к серверу в режиме службы
                Console.WriteLine("AppID   = " + appID);
                Console.WriteLine("Имя exe = " + ExeName);
                if (appID != null)
                {
                    using (var view = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64))
                    {
                        using (var route = view.OpenSubKey("AppID", true))
                        {
                            // удаляем хвосты
                            foreach (var name in route.GetSubKeyNames())
							{
                                string defValue = route.OpenSubKey(name)?.GetValue(string.Empty)?.ToString() ?? null;
                                if (name == appID || name == ExeName || defValue == appID || defValue == ExeName)
								{
                                    route.DeleteSubKeyTree(name);
								}
							}

                            // новая запись
                            route.CreateSubKey(appID);
                            var record = route.OpenSubKey(appID, true);
                            record.SetValue(string.Empty, ServerName);
                            record.SetValue("AuthenticationLevel", 1);
                            record.SetValue("LocalService", ServerName);
                            record.SetValue("ServiceParameters", "-Service");

                            // новая запись
                            route.CreateSubKey(AppDomain.CurrentDomain.FriendlyName);
                            record = route.OpenSubKey(AppDomain.CurrentDomain.FriendlyName, true);
                            record.SetValue(string.Empty, AppDomain.CurrentDomain.FriendlyName);
                            record.SetValue("AppID", appID);
                            record.SetValue("LocalService", ServerName);
                            record.SetValue("ServiceParameters", "-Service");
                        }
                    }
                }

                Console.WriteLine("Инициализация DCOM завершена");
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка при инициализация DCOM: " + e.Message);
            }

            RefreshAllClients();
        }

        public static void UnknownItem(string _, string path)
        {
            Write(path, null);
        }

        public static void Write(string path, object value = null, ushort quality = 0)
        {
            lock (Tags)
            {
                if (Tags.ContainsKey(path))
                {
                    UpdateTag(Tags[path], value, quality);
                }
                else
                {
                    Tags[path] = CreateTag(path, value, quality, true);
                }
            }
        }

        public static void CleanOldTags()
        {
            lock (Program.Configuration)
            {
                foreach (var driver in Program.Configuration.Drivers)
                {
                    foreach (var device in driver.Devices)
                    {
                        var fieldsToRemove = Tags.Keys.Where(x => x.Contains(driver.Name + "." + device.Name + ".")).ToList();
                        var fields = device.Fields().Keys.ToList();

                        foreach (var field in fields)
                        {
                            if (fieldsToRemove.Contains(field))
                            {
                                fieldsToRemove.Remove(field);
                            }
                        }

                        foreach (var field in fieldsToRemove)
                        {
                            var address = Tags[field];
                            RemoveTag(address);
                            Tags.Remove(field);
                        }
                    }
                }
            }
        }


        // Internal methods

        [DllImport("iNOPC_Server.dll")]
        public static extern bool InitWTOPCsvr([MarshalAs(UnmanagedType.LPStr)] string CLSID_Svr, uint ServerRate);

        [DllImport("iNOPC_Server.dll")]
        public static extern bool UpdateRegistry([MarshalAs(UnmanagedType.LPStr)] string CLSID_Svr, [MarshalAs(UnmanagedType.LPStr)] string Name, [MarshalAs(UnmanagedType.LPStr)] string Descr, [MarshalAs(UnmanagedType.LPStr)] string ExePath);

        [DllImport("iNOPC_Server.dll")]
        public static extern void SetVendorInfo([MarshalAs(UnmanagedType.LPStr)] string VendorInfo);

        [DllImport("iNOPC_Server.dll")]
        public static extern bool Deactivate30MinTimer([MarshalAs(UnmanagedType.LPStr)] string Key);

        [DllImport("iNOPC_Server.dll")]
        public static extern bool UnregisterServer([MarshalAs(UnmanagedType.LPStr)] string CLSID_Svr, [MarshalAs(UnmanagedType.LPStr)] string Name);

        [DllImport("iNOPC_Server.dll")]
        public static extern bool UninitWTOPCsvr();

        [DllImport("iNOPC_Server.dll")]
        public static extern void RequestDisconnect();

        [DllImport("iNOPC_Server.dll")]
        public static extern bool RefreshAllClients();

        [DllImport("iNOPC_Server.dll")]
        public static extern short WTOPCsvrRevision();

        [DllImport("iNOPC_Server.dll")]
        public static extern bool EnableUnknownItemNotification(UnknownItemDelegate Callback);

        [DllImport("iNOPC_Server.dll")]
        public static extern uint CreateTag([MarshalAs(UnmanagedType.LPStr)] string Name, object Value, ushort InitialQuality, bool IsWritable);

        [DllImport("iNOPC_Server.dll")]
        public static extern bool UpdateTag(uint TagHandle, object Value, ushort Quality);

        [DllImport("iNOPC_Server.dll")]
        public static extern bool RemoveTag(uint TagHandle);

        [DllImport("iNOPC_Server.dll")]
        public static extern bool ReadTag(uint TagHandle, ref object Value);

        public delegate void UnknownItemDelegate([MarshalAs(UnmanagedType.LPStr)] string PathName, [MarshalAs(UnmanagedType.LPStr)] string ItemName);

        [DllImport("iNOPC_Server.dll")]
        public static extern bool EnableWriteNotification(WriteNotificationDelegate Callback, bool ConvertToNativeType);

        public delegate void WriteNotificationDelegate(uint hItem, ref object Value, ref uint ResultCode);
    }
}