using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace iNOPC.Server
{
    public static class OPC
    {
        public const string ServerName = "iNOPC";
        public const string Pass = "JVRPS53R5V64226N62H4";
        public const string CLSID = "{4537357b-3739-3334-432d-303637382d34}";
        public const string APPID = "{3335347b-3337-3735-622d-333733392d33}";

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

        /// <summary>
        /// Команда на изменение значения извне
        /// </summary>
        /// <param name="item">Путь к тегу</param>
        /// <param name="value">Значение</param>
        /// <param name="result">Результат: 1 (драйвер не найден), 2 (устройство не найдено), 0 (команда передана устройству)</param>
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

        public static void UninstallDCOM(bool silent = false)
        {
            if (!silent) Program.Log("Отмена регистрации в DCOM...");

            try
            {
                // нужно найти и удалить все упоминания iNOPC в реестре
                using (var view = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64))
                {
                    try
                    {
                        using (var route = view.OpenSubKey("AppID", true))
                        {
                            // удаляем хвосты
                            foreach (var name in route.GetSubKeyNames())
                            {
                                string defValue = route.OpenSubKey(name)?.GetValue(string.Empty)?.ToString() ?? null;
                                if (name == Program.ExeName || defValue == Program.ExeName || name == ServerName || defValue == ServerName)
                                {
                                    route.DeleteSubKeyTree(name);
                                }
                            }
                        }
                    }
                    catch { }

                    try
                    {
                        using (var route = view.OpenSubKey("WOW6432Node\\CLSID", true))
                        {
                            // удаляем хвосты
                            foreach (var name in route.GetSubKeyNames())
                            {
                                string defValue = route.OpenSubKey(name)?.GetValue(string.Empty)?.ToString() ?? null;
                                if (name == Program.ExeName || defValue == Program.ExeName || name == ServerName || defValue == ServerName)
                                {
                                    route.DeleteSubKeyTree(name);
                                }
                            }
                        }
                    }
                    catch { }

                    // удаляем хвосты
                    try
                    {
                        foreach (var name in view.GetSubKeyNames())
                        {
                            string defValue = view.OpenSubKey(name)?.GetValue(string.Empty)?.ToString() ?? null;
                            if (name == Program.ExeName || defValue == Program.ExeName || name == ServerName || defValue == ServerName)
                            {
                                view.DeleteSubKeyTree(name);
                            }
                        }
                    }
                    catch { }
                }

                if (!silent) Program.Log("Регистрация в DCOM отменена");
            }
            catch (Exception e)
			{
                Program.Log("Ошибка при отмене регистрации в DCOM: " + e.Message);
            }
        }

        public static void InstallDCOM()
        {
            Program.Log("Регистрация в DCOM ...");

            UninstallDCOM(true);

            RequestDisconnect();
            UnregisterServer(CLSID, ServerName);
            UpdateRegistry(CLSID, ServerName, ServerName, Environment.CurrentDirectory + "\\" + Program.ExeName);
            SetVendorInfo("iNOPC RUP Vitebskenergo");

			try
			{
				using (var view = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64))
				{
					using (var route = view.OpenSubKey("AppID", true))
					{
						// новая запись
						route.CreateSubKey(APPID);
						var record = route.OpenSubKey(APPID, true);
						record.SetValue(string.Empty, ServerName);
						record.SetValue("AuthenticationLevel", 1);
						record.SetValue("LocalService", ServerName);
						record.SetValue("ServiceParameters", "-Service");

						// новая запись
						route.CreateSubKey(Program.ExeName);
						record = route.OpenSubKey(Program.ExeName, true);
						record.SetValue(string.Empty, Program.ExeName);
						record.SetValue("AppID", APPID);
						record.SetValue("LocalService", ServerName);
						record.SetValue("ServiceParameters", "-Service");
					}
				}

				Program.Log("Сервер зарегистрирован в DCOM");
			}
			catch (Exception e)
			{
				Program.Log("Ошибка при регистрации в DCOM: " + e.Message);
			}

			InitWTOPCsvr(CLSID, 1000);
            Deactivate30MinTimer(Pass);
            RefreshAllClients();
        }

        public static void UnknownItem(string _, string path)
        {
            Write(path, null);
        }

        /// <summary>
        /// Чтение тега из OPC
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static object Read(string path)
        {
            if (!Tags.ContainsKey(path)) return 0;

            object v = 0;
            ReadTag(Tags[path], ref v);
            return v;
        }

        /// <summary>
        /// Запись нового значения в OPC
        /// </summary>
        /// <param name="path"></param>
        /// <param name="value"></param>
        /// <param name="quality"></param>
        public static void Write(string path, object value = null, ushort quality = 0)
        {
            if (!string.IsNullOrEmpty(path))
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
        }

        /// <summary>
        /// Удаление тега из OPC
        /// </summary>
        /// <param name="path"></param>
        public static void Remove(string path)
        {
            lock (Tags)
            {
                if (Tags.ContainsKey(path))
                {
                    RemoveTag(Tags[path]);
                    Tags.Remove(path);
                }
            }
        }

        public static void CleanOldTags()
        {
            lock (Program.Configuration)
            {
                foreach (var tag in Tags)
				{
                    RemoveTag(tag.Value);
                    Tags.Remove(tag.Key);
				}

                foreach (var driver in Program.Configuration.Drivers)
                {
                    driver.Load();
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