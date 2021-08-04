using System;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace IEC_TEST
{
	class Program
    {
        static void Main()
		{
            var Port = new SerialPort
            {
                PortName = "COM9",
                BaudRate = 2400,
                DataBits = 8,
                StopBits = StopBits.One,
                Parity = Parity.None,
            };

			Port.DataReceived += Port_DataReceived;
            Port.Open();

            Port.Write("Q1" + (char)0x0D);
            Console.WriteLine("TX: Q1");

            Task.Delay(2000).Wait();

            byte[] buffer = new byte[Port.BytesToRead];
            Port.Read(buffer, 0, buffer.Length);
            string answer = Encoding.UTF8.GetString(buffer);
            Console.WriteLine("RX: " + answer);

            Console.ReadLine();

            void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                byte[] answer1 = new byte[Port.BytesToRead];
                Port.Read(answer1, 0, answer1.Length);
                Console.WriteLine("RX: " + BytesToString(answer1));
            }
        }

		public static string BytesToString(byte[] bytes)
        {
            string s = "";
            if (bytes.Length > 0)
            {
                for (int i = 0; i < bytes.Length; i++) s += bytes[i].ToString("X2") + " ";
                s = s.Substring(0, s.Length - 1);
            }
            return s;
        }
    }
}