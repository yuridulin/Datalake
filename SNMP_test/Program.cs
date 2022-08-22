using SnmpSharpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SNMP_test
{
	internal class Program
	{
		static void Main(string[] args)
		{
			VbCollection vbCol = new VbCollection();
			vbCol.Add("1.3.6.1.2.1.1.1.0");
			//vbCol.Add("1.3.6.1.2.1.1.2.0");
			//vbCol.Add("1.3.6.1.2.1.1.1.1.0");
			Pdu getPdu = Pdu.GetPdu(vbCol);
			UdpTarget target = new UdpTarget(IPAddress.Parse("10.178.9.57")/*IPAddress.Loopback*/);
			AgentParameters agentParams =
				new AgentParameters(SnmpVersion.Ver1, new OctetString("public"));
			SnmpV1Packet response = (SnmpV1Packet)target.Request(getPdu, agentParams);
			if (response != null)
			{
				if (response.Pdu.ErrorStatus == (int)PduErrorStatus.noError)
				{
					Console.WriteLine("Response id {0}\n1: {1}",
						response.Pdu.RequestId,
						response.Pdu.VbList[0].Value.ToString());
					Console.WriteLine("Error status: {0}",
						((PduErrorStatus)response.Pdu.ErrorStatus).ToString());
				}
				else
				{
					Console.WriteLine("Response id {0}", response.Pdu.RequestId);
					Console.WriteLine("Error code: {0} index: {1} name: {2}",
						response.Pdu.ErrorStatus, response.Pdu.ErrorIndex,
						((PduErrorStatus)response.Pdu.ErrorStatus).ToString());
				}
			}
			target.Dispose();

			Console.ReadLine();
		}
	}
}
