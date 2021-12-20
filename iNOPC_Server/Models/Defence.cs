using System;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace iNOPC.Server.Models
{
    class Defence
    {
        public static LicenseMode License { get; set; } = LicenseMode.ActiveTrial;

        public static void Set()
        {
            ProgramStart = DateTime.Now;

            Timer = new Timer(1000);
            Timer.Elapsed += (s, e) => Check();
            Timer.Start();

            Task.Run(Check);
        }

        public static string GetUniqueHardwareId()
        {
            var sb = new StringBuilder();

            // проверка конфигурации сервера
            var searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (var queryObj in searcher.Get())
            {
                sb.Append(queryObj["NumberOfCores"]);
                sb.Append(queryObj["ProcessorId"]);
                sb.Append(queryObj["Name"]);
                sb.Append(queryObj["SocketDesignation"]);
            }

            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BIOS");
            foreach (var queryObj in searcher.Get())
            {
                sb.Append(queryObj["Manufacturer"]);
                sb.Append(queryObj["Name"]);
                sb.Append(queryObj["Version"]);
            }

            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
            foreach (var queryObj in searcher.Get())
            {
                sb.Append(queryObj["Product"]);
            }

            // вычисление хэша-идентификатора
            var bytes = Encoding.ASCII.GetBytes(sb.ToString());
            var sha = new SHA256Managed();
            var hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }


        private static DateTime ProgramStart { get; set; } = DateTime.MinValue;

        private static Timer Timer { get; set; }

        private static void Check()
        {
            if (Enc(GetUniqueHardwareId(), "SecretKeyiNOPC") == Program.Configuration.Key)
            {
                License = LicenseMode.Licensed;
                Timer.Stop();
            }
            else if ((DateTime.Now - ProgramStart) < TimeSpan.FromHours(4))
            {
                License = LicenseMode.ActiveTrial;
            }
            else
            {
                License = LicenseMode.ExpiredTrial;
                Program.SweetStop();
            }
        }

        static string Enc(string plaintext, string pad)
        {
            var data = Encoding.UTF8.GetBytes(plaintext);
            var key = Encoding.UTF8.GetBytes(pad);

            return Convert.ToBase64String(data.Select((b, i) => (byte)(b ^ key[i % key.Length])).ToArray());
        }

        static string Dec(string enctext, string pad)
        {
            var data = Convert.FromBase64String(enctext);
            var key = Encoding.UTF8.GetBytes(pad);

            return Encoding.UTF8.GetString(data.Select((b, i) => (byte)(b ^ key[i % key.Length])).ToArray());
        }
    }

    enum LicenseMode
    {
        ActiveTrial,
        ExpiredTrial,
        Licensed,
    }
}