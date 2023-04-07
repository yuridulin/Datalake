using Datalake.Collector.Models;
using Datalake.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Collector
{
	public class CollectorWorker
	{
		public static async Task Start(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					RebuildPackets();
					UpdateFromPackets();
				}
				catch (Exception ex)
				{
					Console.WriteLine(DateTime.Now + " [" + nameof(CollectorWorker) + "] " + ex.ToString());
				}

				await Task.Delay(1000);
			}
		}


		public static List<SourcePacket> Packets { get; set; } = new List<SourcePacket>();

		public static DateTime StoredUpdate { get; set; } = DateTime.MinValue;

		public static void RebuildPackets()
		{
			using (var db = new DatabaseContext())
			{
				var lastUpdate = db.GetUpdateDate();
				if (lastUpdate == StoredUpdate) return;

				Console.WriteLine("Выполняется пересборка пакетов обновления");

				var tags = db.Tags
					.ToList();

				var sources = db.Sources
					.ToList();

				Packets.Clear();

				foreach (var source in sources)
				{
					var packet = new SourcePacket
					{
						Address = source.Address,
						Tags = tags
							.Where(x => x.SourceId == source.Id)
							.Select(x => new SourceItem
							{
								TagName = x.TagName,
								ItemName = x.SourceItem,
								Interval = TimeSpan.FromSeconds(x.Interval)
							})
							.ToList()
					};

					Packets.Add(packet);
				}

				StoredUpdate = lastUpdate;
			}
		}

		public static void UpdateFromPackets()
		{
			foreach (var packet in Packets)
			{
				packet.Update();
			}
		}
	}
}
