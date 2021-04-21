using lib60870.CS101;
using lib60870.CS104;
using System;
using System.Linq;

namespace IEC_TEST
{
	class Program
	{
		static void Main()
		{
			Console.WriteLine("a");

			//84 02 BB F0
			Console.WriteLine("84 02 BB F0" + " = " + BitConverter.ToSingle(new byte[] { 0x84, 0x02, 0xBB, 0xF0 }, 0));
			Console.WriteLine("BB F0 84 02" + " = " + BitConverter.ToSingle(new byte[] { 0xBB, 0xF0, 0x84, 0x02 }, 0));
			Console.WriteLine("02 84 F0 BB" + " = " + BitConverter.ToSingle(new byte[] { 0x02, 0x84, 0xF0, 0xBB }, 0));
			Console.WriteLine("F0 BB 02 84" + " = " + BitConverter.ToSingle(new byte[] { 0xF0, 0xBB, 0x02, 0x84 }, 0));

			Console.ReadLine();
		}
	}

	class XProgram
    {
        static void XMain(string[] args)
		{
			Console.Write("Enter IP: ");
			var ip = Console.ReadLine();

            var conn = new Connection(ip, 2404)
            {
                DebugOutput = false,
                Autostart = true,
            };

			conn.SetConnectionHandler(ConnectionHandler, null);
			//conn.SetASDUReceivedHandler(AsduReceivedHandler, null);
			conn.SetSentRawMessageHandler(RawMessageHandler, " > ");
			conn.SetReceivedRawMessageHandler(RawMessageHandler, " < ");

            conn.Connect();

			//conn.SendInterrogationCommand(CauseOfTransmission.ACTIVATION, 1, QualifierOfInterrogation.STATION);

			Console.ReadLine();
		}

		private static void ConnectionHandler(object parameter, ConnectionEvent connectionEvent)
		{
			switch (connectionEvent)
			{
				case ConnectionEvent.OPENED:
					Console.WriteLine("Connected");
					break;
				case ConnectionEvent.CLOSED:
					Console.WriteLine("Connection closed");
					break;
				case ConnectionEvent.STARTDT_CON_RECEIVED:
					Console.WriteLine("STARTDT CON received");
					break;
				case ConnectionEvent.STOPDT_CON_RECEIVED:
					Console.WriteLine("STOPDT CON received");
					break;
			}
		}

		private static bool AsduReceivedHandler(object parameter, ASDU asdu)
		{
			Console.WriteLine(asdu.ToString());

			if (asdu.TypeId == TypeID.M_SP_NA_1)
			{

				for (int i = 0; i < asdu.NumberOfElements; i++)
				{

					var val = (SinglePointInformation)asdu.GetElement(i);

					Console.WriteLine("  IOA: " + val.ObjectAddress + " SP value: " + val.Value);
					Console.WriteLine("   " + val.Quality.ToString());
				}
			}
			else if (asdu.TypeId == TypeID.M_ME_TE_1)
			{

				for (int i = 0; i < asdu.NumberOfElements; i++)
				{

					var msv = (MeasuredValueScaledWithCP56Time2a)asdu.GetElement(i);

					Console.WriteLine("  IOA: " + msv.ObjectAddress + " scaled value: " + msv.ScaledValue);
					Console.WriteLine("   " + msv.Quality.ToString());
					Console.WriteLine("   " + msv.Timestamp.ToString());
				}

			}
			else if (asdu.TypeId == TypeID.M_ME_TF_1)
			{

				for (int i = 0; i < asdu.NumberOfElements; i++)
				{
					var mfv = (MeasuredValueShortWithCP56Time2a)asdu.GetElement(i);

					Console.WriteLine("  IOA: " + mfv.ObjectAddress + " float value: " + mfv.Value);
					Console.WriteLine("   " + mfv.Quality.ToString());
					Console.WriteLine("   " + mfv.Timestamp.ToString());
					Console.WriteLine("   " + mfv.Timestamp.GetDateTime().ToString());
				}
			}
			else if (asdu.TypeId == TypeID.M_SP_TB_1)
			{

				for (int i = 0; i < asdu.NumberOfElements; i++)
				{

					var val = (SinglePointWithCP56Time2a)asdu.GetElement(i);

					Console.WriteLine("  IOA: " + val.ObjectAddress + " SP value: " + val.Value);
					Console.WriteLine("   " + val.Quality.ToString());
					Console.WriteLine("   " + val.Timestamp.ToString());
				}
			}
			else if (asdu.TypeId == TypeID.M_ME_NC_1)
			{

				for (int i = 0; i < asdu.NumberOfElements; i++)
				{
					var mfv = (MeasuredValueShort)asdu.GetElement(i);

					Console.WriteLine("  IOA: " + mfv.ObjectAddress + " float value: " + mfv.Value);
					Console.WriteLine("   " + mfv.Quality.ToString());
				}
			}
			else if (asdu.TypeId == TypeID.M_ME_NB_1)
			{

				for (int i = 0; i < asdu.NumberOfElements; i++)
				{

					var msv = (MeasuredValueScaled)asdu.GetElement(i);

					Console.WriteLine("  IOA: " + msv.ObjectAddress + " scaled value: " + msv.ScaledValue);
					Console.WriteLine("   " + msv.Quality.ToString());
				}

			}
			else if (asdu.TypeId == TypeID.M_ME_ND_1)
			{

				for (int i = 0; i < asdu.NumberOfElements; i++)
				{

					var msv = (MeasuredValueNormalizedWithoutQuality)asdu.GetElement(i);

					Console.WriteLine("  IOA: " + msv.ObjectAddress + " scaled value: " + msv.NormalizedValue);
				}

			}
			else if (asdu.TypeId == TypeID.C_IC_NA_1)
			{
				if (asdu.Cot == CauseOfTransmission.ACTIVATION_CON)
					Console.WriteLine((asdu.IsNegative ? "Negative" : "Positive") + "confirmation for interrogation command");
				else if (asdu.Cot == CauseOfTransmission.ACTIVATION_TERMINATION)
					Console.WriteLine("Interrogation command terminated");
			}
			else if (asdu.TypeId == TypeID.F_DR_TA_1)
			{
				Console.WriteLine("Received file directory:\n------------------------");
				int ca = asdu.Ca;

				for (int i = 0; i < asdu.NumberOfElements; i++)
				{
					FileDirectory fd = (FileDirectory)asdu.GetElement(i);

					Console.Write(fd.FOR ? "DIR:  " : "FILE: ");

					Console.WriteLine("CA: {0} IOA: {1} Type: {2}", ca, fd.ObjectAddress, fd.NOF.ToString());
				}

			}
			else
			{
				Console.WriteLine("Unknown message type!");
			}

			return true;
		}

		static bool RawMessageHandler(object parameter, byte[] message, int messageSize)
		{
			Console.WriteLine(parameter + BytesToString(message.Take(messageSize).ToArray()));

			return true;
		}

		static string BytesToString(byte[] bytes)
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
