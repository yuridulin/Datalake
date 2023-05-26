using LinqToDB;
using Logger.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Logger.Workers
{
	public static class NetworkWorker
	{
		public static async Task Work(CancellationToken token)
		{
			var ping = new Ping();
			var host = Dns.GetHostEntry(Dns.GetHostName()); 
			var networks = new List<string>();

			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					string mask = ip.ToString();
					mask = mask.Substring(0, mask.LastIndexOf('.'));
					networks.Add(mask);
				}
			}

			var endpoints = new List<string>();

			while (!token.IsCancellationRequested)
			{
				endpoints.Clear();

				foreach (var mask in networks)
				{
					for (int i = 1; i <= 254; i++)
					{
						string ip = mask + "." + i;
						//Console.WriteLine("Пинг: " + ip);

						var reply = ping.Send(ip, 150);
						if (reply.Status == IPStatus.Success)
						{
							try
							{
								string endpoint = Dns.GetHostEntry(ip).HostName;
								endpoint = endpoint.Substring(0, endpoint.IndexOf('.'));

								endpoints.Add(endpoint);

								//Console.WriteLine("Найдена точка: [" + ip + "] " + endpoint);
							}
							catch { }
						}
					}
				}

				using (var db = new DatabaseContext())
				{
					foreach (var endpoint in endpoints)
					{
						if (!string.IsNullOrEmpty(endpoint) && !db.Stations.Any(x => x.Endpoint == endpoint))
						{
							db.Stations
								.Value(x => x.Endpoint, endpoint.ToUpper())
								.Value(x => x.Description, "Запись создана автоматически")
								.Value(x => x.StationConfigId, 0)
								.Value(x => x.LastTimeAlive, DateTime.MinValue)
								.Value(x => x.DeployStatus, StationDeployState.No_Info)
								.Value(x => x.DeployTime, DateTime.MinValue)
								.Insert();

							//Console.WriteLine("Создана запись: " + endpoint);
						}
					}
				}

				// ожидание следующего подхода
				await Task.Delay(TimeSpan.FromMinutes(10));
			}
		}
	}
}

