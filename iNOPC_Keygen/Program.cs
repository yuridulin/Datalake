using System;
using System.Linq;
using System.Text;

namespace iNOPC_Keygen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                Console.Write("Id: ");
                string id = Console.ReadLine();

                Console.WriteLine("Key: " + Enc(id, "SecretKeyiNOPC"));
            }
        }

        static string Enc(string plaintext, string pad)
        {
            var data = Encoding.UTF8.GetBytes(plaintext);
            var key = Encoding.UTF8.GetBytes(pad);

            return Convert.ToBase64String(data.Select((b, i) => (byte)(b ^ key[i % key.Length])).ToArray());
        }
    }
}