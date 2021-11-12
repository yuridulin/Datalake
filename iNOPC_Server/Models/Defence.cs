using System;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace iNOPC.Server.Models
{
	class Defence
	{
        private static string GetUniqueHardwareId()
        {
            StringBuilder sb = new StringBuilder();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2",
                  "SELECT * FROM Win32_Processor");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                sb.Append(queryObj["NumberOfCores"]);
                sb.Append(queryObj["ProcessorId"]);
                sb.Append(queryObj["Name"]);
                sb.Append(queryObj["SocketDesignation"]);

                Console.WriteLine(queryObj["ProcessorId"]);
                Console.WriteLine(queryObj["Name"]);
                Console.WriteLine(queryObj["SocketDesignation"]);
            }

            searcher = new ManagementObjectSearcher("root\\CIMV2",
                "SELECT * FROM Win32_BIOS");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                sb.Append(queryObj["Manufacturer"]);
                sb.Append(queryObj["Name"]);
                sb.Append(queryObj["Version"]);

                Console.WriteLine(queryObj["Manufacturer"]);
                Console.WriteLine(queryObj["Name"]);
                Console.WriteLine(queryObj["Version"]);
            }

            searcher = new ManagementObjectSearcher("root\\CIMV2",
                   "SELECT * FROM Win32_BaseBoard");

            foreach (ManagementObject queryObj in searcher.Get())
            {
                sb.Append(queryObj["Product"]);
                Console.WriteLine(queryObj["Product"]);
            }

            var bytes = Encoding.ASCII.GetBytes(sb.ToString());
            SHA256Managed sha = new SHA256Managed();

            byte[] hash = sha.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        public void ReadLicense()
		{
            string license = File.ReadAllText(Program.Base + "license");
		}
    }
}