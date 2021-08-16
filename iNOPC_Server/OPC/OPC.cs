using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace iNOPC.Server
{
	public static class OPC
    {
        public const string ServerName = "iNOPC";

        public const string CLSID = "{4537357b-3739-3334-432d-303637382d34}";

        public const string Pass = "JVRPS53R5V64226N62H4";

        private static WriteNotificationDelegate _WriteOutDelegate;

        private static UnknownItemDelegate _UnknownItemDelegate;

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
            string pathToExe = Environment.CurrentDirectory + "\\iNOPC_Server.exe";

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
                process.StandardInput.WriteLine("sc create iNOPC binPath=\"" + pathToExe + "\" DisplayName=\"iNOPC\" start=auto && exit");
                process.WaitForExit();

                // Создание записи в DCOM 1
                File.WriteAllText("C:\\dcom.reg", 
                    "Windows Registry Editor Version 5.00\n\n" +
                    "[HKEY_CLASSES_ROOT\\AppID\\{3335347b-3337-3735-622d-333733392d33}]\n" +
                    "@=\"iNOPC\"\n" +
                    "\"AuthenticationLevel\"=dword:00000001\n" +
                    "\"LocalService\"=\"iNOPC\"\n" +
                    "\"ServiceParameters\"=\"-Service\"\n\n");

                process = Process.Start("regedit.exe", "/s C:\\dcom.reg");
                process.WaitForExit();

                // Создание записи в DCOM 2
                File.WriteAllText("C:\\dcom.reg",
                    "Windows Registry Editor Version 5.00\n\n" +
                    "[HKEY_LOCAL_MACHINE\\SOFTWARE\\Classes\\AppID\\iNOPC_Server.exe]\n" +
                    "@=\"iNOPC_Server.exe\"\n" +
                    "\"AppID\"=\"{3335347b-3337-3735-622d-333733392d33}\"\n" +
                    "\"LocalService\"=\"iNOPC\"\n" +
                    "\"ServiceParameters\"=\"-Service\"\n\n");

                process = Process.Start("regedit.exe", "/s C:\\dcom.reg");
                process.WaitForExit();

                // Удаление временного файла
                File.Delete("C:\\dcom.reg");
            }
            catch (Exception e)
			{
                Console.WriteLine("Ошибка при создании записей в реестре: " + e.Message);
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