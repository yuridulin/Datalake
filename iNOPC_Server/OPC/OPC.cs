using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using iNOPC.Server.Models;

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

                    lock (Storage.Drivers)
                    {
                        var driver = Storage.Drivers.FirstOrDefault(x => x.Name == driverName);
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
            RequestDisconnect();
            UnregisterServer(CLSID, ServerName);
            UpdateRegistry(CLSID, ServerName, ServerName, Environment.CurrentDirectory + "\\iNOPC_Server.exe");
            SetVendorInfo("iNOPC RUP Vitebskenergo");
            InitWTOPCsvr(CLSID, 1000);
            Deactivate30MinTimer(Pass);
            RefreshAllClients();
        }

        public static void UnknownItem(string _, string path)
        {
            Write(path, null);
        }

        public static void Write(string path, object value = null)
        {
            if (Tags.ContainsKey(path))
            {
                UpdateTag(Tags[path], value, 192);
            }
            else
            {
                Tags[path] = CreateTag(path, value, 192, true);
            }
        }

        public static void CleanOldTags()
        {
            foreach (var driver in Storage.Drivers)
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