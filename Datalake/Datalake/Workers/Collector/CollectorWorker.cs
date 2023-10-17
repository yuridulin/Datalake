using Datalake.Database;
using Datalake.Workers.Logs;
using Datalake.Workers.Logs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Datalake.Workers.Collector
{
	public static class CollectorWorker
	{
		public static async Task Start(CancellationToken token)
		{
			while (!token.IsCancellationRequested)
			{
				try
				{
					using (var db = new DatabaseContext())
					{
						if (Cache.LastUpdate > StoredUpdate)
						{
							Sources = db.Sources.ToList();
							Sources.ForEach(source => source.Rebuild(db));
							StoredUpdate = Cache.LastUpdate;
						}

						Sources.ForEach(source => source.Update(db));
					}
				}
				catch (Exception ex)
				{
					LogsWorker.Add("Collector", "Loop error: " + ex.Message, LogType.Error);
				}

				await Task.Delay(1000);
			}
		}

		static DateTime StoredUpdate { get; set; } = DateTime.MinValue;

		static List<Source> Sources { get; set; } = new List<Source>();
	}
}
